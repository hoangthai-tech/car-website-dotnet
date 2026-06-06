using CarWebsite.Data;
using CarWebsite.Models;
using CarWebsite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

public class DashboardController(AppDbContext db, EmailService emailSvc) : Controller
{
    // ── Workflow: trạng thái tiếp theo hợp lệ (chỉ tiến, không lùi) ─────────
    private static readonly Dictionary<string, string[]> _buyNextStatus = new() {
        ["Khách đang xem xe"]               = ["Đang lái thử", "Đã đặt cọc", "Đã hủy"],
        ["Chờ xử lý"]                       = ["Đang lái thử", "Đã đặt cọc", "Đã hủy"],
        ["Đang lái thử"]                    = ["Đã đặt cọc", "Đã hủy"],
        ["Đã đặt cọc"]                      = ["Đang giao", "Đã hủy"],
        ["Chờ kế toán duyệt tiền"]          = ["Đã đặt cọc — Chờ thanh toán đủ", "Chờ bàn giao xe", "Đã hủy"],
        ["Đã đặt cọc — Chờ thanh toán đủ"] = ["Chờ bàn giao xe", "Đã hủy"],
        ["Chờ bàn giao xe"]                 = ["Đang giao", "Đã hủy"],
        ["Đang giao"]                       = ["Hoàn tất"],
        ["Hoàn tất"]                        = [],
        ["Đã hủy"]                          = [],
    };
    private static readonly Dictionary<string, string[]> _rentNextStatus = new() {
        ["Chờ xác nhận"] = ["Đã xác nhận", "Đã hủy"],
        ["Đã xác nhận"]  = ["Đang thuê", "Đã hủy"],
        ["Đang thuê"]    = ["Đã trả xe"],
        ["Đã trả xe"]    = [],
        ["Đã hủy"]       = [],
    };

    // ── Helpers (session-based, tương thích cũ) ─────────────────────────────
    private string? Role => HttpContext.Session.GetString("UserRole");

    private bool IsStaff()      => Roles.AllStaff.Contains(Role ?? "");
    private bool IsAdmin()      => Role == Roles.Admin;
    private bool IsManager()    => Role == Roles.Admin || Role == Roles.Manager;
    private bool IsSale()       => Role == Roles.Admin || Role == Roles.Sale;
    private bool IsWarehouse()  => Role == Roles.Admin || Role == Roles.Warehouse;
    private bool IsAccounting() => Role == Roles.Admin || Role == Roles.Accounting;
    private bool IsService()    => Role == Roles.Admin || Role == Roles.Service;

    public async Task<IActionResult> Index()
    {
        if (!IsStaff()) return RedirectToAction("Login", "Account", new { returnUrl = "/Dashboard" });

        var now   = DateTime.UtcNow;
        var month = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // 4 thẻ KPI
        ViewBag.RevenueThisMonth  = await db.Orders
            .Where(o => o.Status == "Hoàn tất" && o.UpdatedAt >= month)
            .SumAsync(o => (long?)o.Amount) ?? 0;
        ViewBag.DeliveredThisMonth = await db.Orders
            .CountAsync(o => o.Status == "Hoàn tất" && o.UpdatedAt >= month);
        ViewBag.CarsInStock = await db.VehicleUnits.CountAsync(v => v.Status == "available");
        ViewBag.NewCustomersThisMonth = await db.Users
            .CountAsync(u => u.Role == Roles.Customer && u.CreatedAt >= month);

        // Doanh thu 12 tháng — cho Line Chart
        var revenueByMonth = await db.Orders
            .Where(o => o.Status == "Hoàn tất" && o.UpdatedAt >= now.AddMonths(-11))
            .GroupBy(o => new { o.UpdatedAt.Year, o.UpdatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => (long)o.Amount) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();
        ViewBag.RevenueByMonth = revenueByMonth;

        // Tỷ trọng doanh số theo dòng xe — cho Pie Chart
        var salesByType = await db.Orders
            .Where(o => o.Status == "Hoàn tất" && o.CarId != null)
            .Join(db.Cars, o => o.CarId, c => c.Id, (o, c) => new { c.Type, o.Amount })
            .GroupBy(x => x.Type)
            .Select(g => new { Type = g.Key, Total = g.Sum(x => (long)x.Amount) })
            .OrderByDescending(x => x.Total)
            .ToListAsync();
        ViewBag.SalesByType = salesByType;

        // KPI nhân viên kinh doanh
        var kpi = await db.Orders
            .Where(o => o.StaffId != null && o.Status == "Hoàn tất" && o.OrderType == "buy")
            .GroupBy(o => o.StaffId!.Value)
            .Select(g => new { StaffId = g.Key, Count = g.Count(), Revenue = g.Sum(o => (long)o.Amount) })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();
        var staffIds = kpi.Select(k => k.StaffId).ToList();
        var staffMap = await db.Users.Where(u => staffIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name);
        ViewBag.KpiData  = kpi;
        ViewBag.StaffMap = staffMap;

        ViewBag.RecentOrders = await db.Orders.OrderByDescending(o => o.CreatedAt).Take(8).ToListAsync();

        return View();
    }

    // /Dashboard/Xe — quản lý kho xe (admin only)
    public async Task<IActionResult> Xe()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var cars = await db.Cars.OrderByDescending(c => c.CreatedAt).ToListAsync();

        var orderStats = await db.Orders
            .Where(o => o.CarId != null)
            .GroupBy(o => new { o.CarId, o.Status })
            .Select(g => new { g.Key.CarId, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        ViewBag.SoldCount    = orderStats.Where(x => x.Status == "Hoàn tất")
                                         .ToDictionary(x => x.CarId!.Value, x => x.Count);
        ViewBag.DepositCount = orderStats.Where(x => x.Status == "Đã đặt cọc")
                                         .ToDictionary(x => x.CarId!.Value, x => x.Count);
        ViewBag.TestCount    = orderStats.Where(x => x.Status == "Đang lái thử")
                                         .ToDictionary(x => x.CarId!.Value, x => x.Count);

        return View(cars);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(30_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30_000_000)]
    public async Task<IActionResult> XeAdd(string name, string brand, string type, string fuel, int year,
        long price, long rentalPricePerDay, string? image, string? badge, int stock = 1, IFormFile? imageFile = null)
    {
        if (!IsAdmin()) return Forbid();

        var slug = System.Text.RegularExpressions.Regex.Replace(name.ToLower(), @"[^a-z0-9]+", "-").Trim('-');
        var originalSlug = slug;
        int counter = 1;
        while (await db.Cars.AnyAsync(c => c.Slug == slug))
            slug = $"{originalSlug}-{counter++}";

        var priceDisplay = price.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + " ₫";
        var userId = HttpContext.Session.GetString("UserId");
        var carId = Guid.NewGuid();
        var imageUrl = image ?? "";

        if (imageFile != null && imageFile.Length > 0)
        {
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (ext is ".jpg" or ".jpeg" or ".png" or ".webp")
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cars");
                Directory.CreateDirectory(dir);
                var fileName = $"{carId}_main{ext}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await imageFile.CopyToAsync(stream);
                imageUrl = $"/images/cars/{fileName}";
            }
        }

        var car = new Car
        {
            Id = carId,
            Slug = slug,
            Name = name,
            Brand = brand,
            Type = type,
            Fuel = fuel,
            Year = year,
            Price = price,
            PriceDisplay = priceDisplay,
            RentalPricePerDay = rentalPricePerDay,
            Image = imageUrl,
            Badge = string.IsNullOrWhiteSpace(badge) ? null : badge,
            Stock = stock < 1 ? 1 : stock,
            Status = "pending",
            CreatedById = Guid.TryParse(userId, out var uid) ? uid : null
        };

        db.Cars.Add(car);
        await db.SaveChangesAsync();
        await LogAsync("Thêm xe", name);

        TempData["Success"] = $"Đã thêm xe {name}. Xe đang chờ duyệt.";
        return RedirectToAction("Xe");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(30_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30_000_000)]
    public async Task<IActionResult> XeEdit(Guid id, string name, string brand, string type, string fuel,
        int year, long price, long rentalPricePerDay, string? image, string? badge, int stock = 1, IFormFile? imageFile = null)
    {
        if (!IsAdmin()) return Forbid();
        var car = await db.Cars.FindAsync(id);
        if (car == null) { TempData["Error"] = "Không tìm thấy xe."; return RedirectToAction("Xe"); }

        if (imageFile != null && imageFile.Length > 0)
        {
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (ext is ".jpg" or ".jpeg" or ".png" or ".webp")
            {
                var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cars");
                Directory.CreateDirectory(dir);
                var fileName = $"{car.Id}_main{ext}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await imageFile.CopyToAsync(stream);
                car.Image = $"/images/cars/{fileName}";
            }
        }
        else if (!string.IsNullOrWhiteSpace(image))
        {
            car.Image = image;
        }

        car.Name = name;
        car.Brand = brand;
        car.Type = type;
        car.Fuel = fuel;
        car.Year = year;
        car.Price = price;
        car.PriceDisplay = price.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + " ₫";
        car.RentalPricePerDay = rentalPricePerDay;
        car.Badge = string.IsNullOrWhiteSpace(badge) ? null : badge;
        car.Stock = stock < 1 ? 1 : stock;

        await db.SaveChangesAsync();
        await LogAsync("Sửa xe", name);

        TempData["Success"] = $"Đã cập nhật xe {name}.";
        return RedirectToAction("Xe");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(500_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
    public async Task<IActionResult> XeUpload3D(Guid id, IFormFile model3d)
    {
        if (!IsAdmin()) return Forbid();

        var car = await db.Cars.FindAsync(id);
        if (car == null) { TempData["Error"] = "Không tìm thấy xe."; return RedirectToAction("Xe"); }

        if (model3d == null || model3d.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file 3D.";
            return RedirectToAction("Xe");
        }

        var ext = Path.GetExtension(model3d.FileName).ToLowerInvariant();
        if (ext != ".glb" && ext != ".gltf")
        {
            TempData["Error"] = "Chỉ hỗ trợ file .glb hoặc .gltf.";
            return RedirectToAction("Xe");
        }

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "models3d");
        Directory.CreateDirectory(dir);

        // Remove old file if exists
        if (!string.IsNullOrEmpty(car.Model3DUrl))
        {
            var old = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", car.Model3DUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
        }

        var fileName = $"{car.Id}{ext}";
        var filePath = Path.Combine(dir, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
            await model3d.CopyToAsync(stream);

        car.Model3DUrl = $"/models3d/{fileName}";
        car.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Upload 3D", car.Name);

        TempData["Success"] = $"Đã upload mô hình 3D cho xe {car.Name}.";
        return RedirectToAction("Xe");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(30_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30_000_000)]
    public async Task<IActionResult> XeUploadImage(Guid id, IFormFile image)
    {
        if (!IsAdmin()) return Forbid();

        var car = await db.Cars.FindAsync(id);
        if (car == null) { TempData["Error"] = "Không tìm thấy xe."; return RedirectToAction("Xe"); }

        if (image == null || image.Length == 0) { TempData["Error"] = "Vui lòng chọn ảnh."; return RedirectToAction("Xe"); }

        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
        {
            TempData["Error"] = "Chỉ hỗ trợ JPG, PNG, WebP.";
            return RedirectToAction("Xe");
        }

        var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cars");
        Directory.CreateDirectory(dir);

        var fileName = $"{car.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(dir, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
            await image.CopyToAsync(stream);

        var imgs = car.Images;
        imgs.Add($"/images/cars/{fileName}");
        car.Images = imgs;
        car.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Thêm ảnh phụ", car.Name);

        TempData["Success"] = $"Đã thêm ảnh phụ cho xe {car.Name}.";
        return RedirectToAction("Xe");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XeDeleteImage(Guid id, string imageUrl)
    {
        if (!IsAdmin()) return Forbid();

        var car = await db.Cars.FindAsync(id);
        if (car == null) { TempData["Error"] = "Không tìm thấy xe."; return RedirectToAction("Xe"); }

        if (imageUrl.StartsWith("/images/cars/"))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
        }

        var imgs = car.Images;
        imgs.Remove(imageUrl);
        car.Images = imgs;
        car.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Xóa ảnh phụ", car.Name);

        TempData["Success"] = "Đã xóa ảnh phụ.";
        return RedirectToAction("Xe");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XeDelete3D(Guid id)
    {
        if (!IsAdmin()) return Forbid();

        var car = await db.Cars.FindAsync(id);
        if (car == null) { TempData["Error"] = "Không tìm thấy xe."; return RedirectToAction("Xe"); }

        if (!string.IsNullOrEmpty(car.Model3DUrl))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", car.Model3DUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            car.Model3DUrl = null;
            car.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            await LogAsync("Xóa 3D", car.Name);
        }

        TempData["Success"] = $"Đã xóa mô hình 3D của xe {car.Name}.";
        return RedirectToAction("Xe");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XeApprove(Guid id)
    {
        if (!IsAdmin()) return Forbid();
        var car = await db.Cars.FindAsync(id);
        if (car != null)
        {
            car.Status = "approved";
            car.ApprovedAt = DateTime.UtcNow;
            var userId = HttpContext.Session.GetString("UserId");
            if (Guid.TryParse(userId, out var uid)) car.ApprovedById = uid;
            await db.SaveChangesAsync();
            await LogAsync("Duyệt xe", car.Name);
        }
        return RedirectToAction("Xe");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XeDelete(Guid id)
    {
        if (!IsAdmin()) return Forbid();
        var car = await db.Cars.FindAsync(id);
        if (car == null) return RedirectToAction("Xe");
        bool hasSold   = await db.Orders.AnyAsync(o => o.CarId == id && o.Status == "Hoàn tất");
        bool hasActive = await db.Orders.AnyAsync(o => o.CarId == id && o.Status != "Đã hủy" && o.Status != "Hoàn tất");
        if (hasSold) {
            TempData["Error"] = $"Không thể xóa '{car.Name}' vì đã có giao dịch hoàn tất liên quan. Xóa xe sẽ ảnh hưởng đến lịch sử giao dịch.";
            return RedirectToAction("Xe");
        }
        if (hasActive) {
            TempData["Error"] = $"Không thể xóa '{car.Name}' vì còn đơn hàng đang xử lý. Hủy hoặc hoàn tất đơn hàng trước.";
            return RedirectToAction("Xe");
        }
        db.Cars.Remove(car);
        await db.SaveChangesAsync();
        await LogAsync("Xóa xe", car.Name);
        TempData["Success"] = $"Đã xóa xe '{car.Name}'.";
        return RedirectToAction("Xe");
    }

    // /Dashboard/DonHang — quản lý đơn mua xe
    public async Task<IActionResult> DonHang()
    {
        if (!IsStaff()) return RedirectToAction("Login", "Account");
        var orders = await db.Orders
            .Where(o => o.OrderType == "buy")
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    // /Dashboard/XeChoThue — quản lý xe cho thuê
    public async Task<IActionResult> XeChoThue(string? filter)
    {
        if (!IsStaff()) return RedirectToAction("Login", "Account");

        var today = DateTime.UtcNow.Date;
        var toActivate = await db.Orders
            .Where(o => o.OrderType == "rent" && o.Status == "Đã xác nhận"
                     && o.RentalStartDate.HasValue && o.RentalStartDate.Value.Date <= today)
            .ToListAsync();
        foreach (var o in toActivate) { o.Status = "Đang thuê"; o.UpdatedAt = DateTime.UtcNow; }
        if (toActivate.Any()) await db.SaveChangesAsync();

        var query = db.Orders.Where(o => o.OrderType == "rent").Include(o => o.Customer).AsQueryable();
        if (!string.IsNullOrEmpty(filter) && filter != "all")
            query = query.Where(o => o.Status == filter);
        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

        ViewBag.PendingCount   = await db.Orders.CountAsync(o => o.OrderType == "rent" && o.Status == "Đang đặt thuê");
        ViewBag.ConfirmedCount = await db.Orders.CountAsync(o => o.OrderType == "rent" && o.Status == "Đã xác nhận");
        ViewBag.ActiveCount    = await db.Orders.CountAsync(o => o.OrderType == "rent" && o.Status == "Đang thuê");
        ViewBag.ReturnedCount  = await db.Orders.CountAsync(o => o.OrderType == "rent" && o.Status == "Đã trả xe");
        ViewBag.TotalRevenue   = await db.Orders
            .Where(o => o.OrderType == "rent" && o.Status == "Đã trả xe")
            .SumAsync(o => (long?)o.Amount) ?? 0;
        ViewBag.Filter = filter ?? "all";

        return View(orders);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XeChoThueConfirm(Guid id, DateTime rentalStartDate, DateTime rentalEndDate)
    {
        if (!IsManager()) return Forbid();
        var order = await db.Orders.FindAsync(id);
        if (order == null) { TempData["Error"] = "Không tìm thấy đơn thuê."; return RedirectToAction("XeChoThue"); }

        int days = (int)(rentalEndDate.Date - rentalStartDate.Date).TotalDays;
        if (days < 1) { TempData["Error"] = "Ngày trả phải sau ngày nhận ít nhất 1 ngày."; return RedirectToAction("XeChoThue"); }

        if (order.CarId.HasValue)
        {
            var car = await db.Cars.FindAsync(order.CarId.Value);
            if (car != null) order.RentalDailyRate = car.RentalPricePerDay;
        }

        order.RentalStartDate = rentalStartDate;
        order.RentalEndDate   = rentalEndDate;
        order.Amount          = order.RentalDailyRate * days;
        order.Status          = "Đã xác nhận";
        order.UpdatedAt       = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Xác nhận thuê xe", $"{order.OrderCode} — {order.CarName}");

        TempData["Success"] = $"Đã xác nhận cho thuê {order.CarName} từ {rentalStartDate:dd/MM/yyyy} đến {rentalEndDate:dd/MM/yyyy}.";
        return RedirectToAction("XeChoThue");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XeChoThueUpdateStatus(Guid id, string status)
    {
        if (!IsStaff()) return Forbid();
        var order = await db.Orders.FindAsync(id);
        if (order == null) return RedirectToAction("XeChoThue");
        if (!_rentNextStatus.TryGetValue(order.Status, out var allowed) || !allowed.Contains(status)) {
            TempData["Error"] = $"Không thể chuyển trạng thái từ '{order.Status}' sang '{status}'.";
            return RedirectToAction("XeChoThue");
        }
        order.Status    = status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Cập nhật thuê xe", $"{order.OrderCode} → {status}");
        return RedirectToAction("XeChoThue");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XeChoThueDelete(Guid id)
    {
        if (!IsStaff()) return Forbid();
        var order = await db.Orders.FindAsync(id);
        if (order != null)
        {
            await LogAsync("Xóa đơn thuê", $"{order.OrderCode} — {order.CustomerName}");
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
        }
        return RedirectToAction("XeChoThue");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DonHangUpdateStatus(Guid id, string status)
    {
        if (!IsStaff()) return Forbid();
        var order = await db.Orders.FindAsync(id);
        if (order == null) return RedirectToAction("DonHang");
        if (!_buyNextStatus.TryGetValue(order.Status, out var allowed) || !allowed.Contains(status)) {
            TempData["Error"] = $"Không thể chuyển trạng thái từ '{order.Status}' sang '{status}'.";
            return RedirectToAction("DonHang");
        }
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Cập nhật đơn hàng", $"{order.OrderCode} → {status}");
        return RedirectToAction("DonHang");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DonHangDelete(Guid id)
    {
        if (!IsStaff()) return Forbid();
        var order = await db.Orders.FindAsync(id);
        if (order != null)
        {
            await LogAsync("Xóa đơn hàng", $"{order.OrderCode} — {order.CustomerName}");
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
        }
        return RedirectToAction("DonHang");
    }

    // /Dashboard/NhanVien — quản lý nhân viên
    public async Task<IActionResult> NhanVien()
    {
        if (!IsManager()) return RedirectToAction("Login", "Account");
        var users = await db.Users.Where(u => u.Role != "customer").ToListAsync();
        return View(users);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NhanVienAdd(string name, string email, string password, string role, string branch)
    {
        if (!IsManager()) return Forbid();

        if (!IsAdmin() && role is "admin" or "manager")
        {
            TempData["Error"] = "Bạn không có quyền tạo tài khoản với vai trò này.";
            return RedirectToAction("NhanVien");
        }

        if (password.Length < 8)
        {
            TempData["Error"] = "Mật khẩu phải có ít nhất 8 ký tự.";
            return RedirectToAction("NhanVien");
        }

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            TempData["Error"] = "Email này đã được sử dụng.";
            return RedirectToAction("NhanVien");
        }

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            Branch = branch
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        await LogAsync("Thêm nhân viên", name);

        TempData["Success"] = $"Đã thêm nhân viên {name} thành công.";
        return RedirectToAction("NhanVien");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NhanVienDelete(Guid id)
    {
        if (!IsAdmin()) return Forbid();
        var user = await db.Users.FindAsync(id);
        if (user != null && user.Role != "admin")
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            await LogAsync("Xóa nhân viên", user.Name);
        }
        return RedirectToAction("NhanVien");
    }

    // /Dashboard/KhachHang — danh sách khách hàng
    public async Task<IActionResult> KhachHang(string? q)
    {
        if (!IsManager()) return RedirectToAction("Login", "Account");

        var query = db.Users.Where(u => u.Role == "customer");
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.Name.Contains(q) || u.Email.Contains(q));

        var customers = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

        var orderStats = await db.Orders
            .Where(o => o.CustomerId != null)
            .GroupBy(o => o.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, Count = g.Count(), Total = g.Sum(o => (long)o.Amount) })
            .ToListAsync();

        var allOrders = await db.Orders
            .Where(o => o.CustomerId != null)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.OrderCounts    = orderStats.ToDictionary(x => x.CustomerId, x => x.Count);
        ViewBag.OrderTotals    = orderStats.ToDictionary(x => x.CustomerId, x => x.Total);
        ViewBag.CustomerOrders = allOrders.GroupBy(o => o.CustomerId!.Value)
                                          .ToDictionary(g => g.Key, g => g.ToList());
        ViewBag.SearchQ = q;
        return View(customers);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KhachHangDelete(Guid id)
    {
        if (!IsAdmin()) return Forbid();

        var user = await db.Users.FindAsync(id);
        if (user == null || user.Role != "customer")
        {
            TempData["Error"] = "Không tìm thấy tài khoản khách hàng.";
            return RedirectToAction("KhachHang");
        }

        string deletedName = user.Name;

        // Ẩn danh hóa đơn hàng — giữ lịch sử nhưng không còn liên kết tới cá nhân
        var orders = await db.Orders.Where(o => o.CustomerId == id).ToListAsync();
        foreach (var o in orders)
        {
            o.CustomerId    = null;
            o.CustomerName  = "Khách hàng đã xóa";
        }

        // Xóa user → email hoàn toàn tự do, có thể đăng ký lại
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        await LogAsync("Xóa khách hàng", deletedName);

        TempData["Success"] = $"Đã xóa tài khoản \"{deletedName}\". Email đã được giải phóng và có thể đăng ký lại.";
        return RedirectToAction("KhachHang");
    }

    // /Dashboard/KhuyenMai — chiết khấu
    public async Task<IActionResult> KhuyenMai()
    {
        if (!IsStaff()) return RedirectToAction("Login", "Account");
        var requests = await db.DiscountRequests.OrderByDescending(d => d.CreatedAt).ToListAsync();
        return View(requests);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KhuyenMaiReview(Guid id, string status, string? note)
    {
        if (!IsManager()) return Forbid();
        var req = await db.DiscountRequests.FindAsync(id);
        if (req != null)
        {
            req.Status = status;
            req.ReviewNote = note;
            var userId = HttpContext.Session.GetString("UserId");
            if (Guid.TryParse(userId, out var uid)) req.ReviewedById = uid;
            await db.SaveChangesAsync();
            await LogAsync("Duyệt chiết khấu", $"{req.CarName} ({status})");
        }
        return RedirectToAction("KhuyenMai");
    }

    // /Dashboard/TestEmail — chỉ admin, test gửi email
    [HttpGet]
    public async Task<IActionResult> TestEmail(string? to)
    {
        if (!IsAdmin()) return Forbid();
        if (string.IsNullOrWhiteSpace(to))
            return Content("Dùng: /Dashboard/TestEmail?to=email@gmail.com");

        try
        {
            await emailSvc.SendVerificationCodeAsync(to, "Test User", "123456");
            return Content($"✅ Email gửi thành công đến {to} — kiểm tra cả hộp Spam và tab Promotions!");
        }
        catch (Exception ex)
        {
            return Content($"❌ Lỗi: {ex.GetType().Name}\n{ex.Message}\n\nInner: {ex.InnerException?.Message}");
        }
    }

    // /Dashboard/AuditLog
    public async Task<IActionResult> AuditLog()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        var logs = await db.AuditLogs.OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync();
        return View(logs);
    }

    // /Dashboard/BaoCao
    public async Task<IActionResult> BaoCao()
    {
        if (!IsManager()) return RedirectToAction("Login", "Account");

        var ordersByStatus = await db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Revenue = g.Sum(o => (long)o.Amount) })
            .ToListAsync();

        ViewBag.OrdersByStatus = ordersByStatus;
        ViewBag.TotalRevenue = await db.Orders.Where(o => o.Status == "Hoàn tất").SumAsync(o => (long?)o.Amount) ?? 0;
        ViewBag.TotalOrders = await db.Orders.CountAsync();
        ViewBag.TopCars = await db.Orders
            .GroupBy(o => o.CarName)
            .Select(g => new { CarName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        return View();
    }

    // /Dashboard/TinNhan — xem tin nhắn liên hệ
    public async Task<IActionResult> TinNhan()
    {
        if (!IsStaff()) return RedirectToAction("Login", "Account");
        var messages = await db.AuditLogs
            .Where(l => l.Action == "Liên hệ mới")
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        return View(messages);
    }

    // /Dashboard/CaiDat
    public IActionResult CaiDat()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CaiDatDoiMatKhau(string currentPassword, string newPassword, string confirmPassword)
    {
        if (!IsAdmin()) return Forbid();

        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "Mật khẩu mới không khớp.";
            return RedirectToAction("CaiDat");
        }
        if (newPassword.Length < 8)
        {
            TempData["Error"] = "Mật khẩu mới phải có ít nhất 8 ký tự.";
            return RedirectToAction("CaiDat");
        }

        var userId = HttpContext.Session.GetString("UserId");
        if (!Guid.TryParse(userId, out var uid)) return Forbid();

        var user = await db.Users.FindAsync(uid);
        if (user == null || user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            TempData["Error"] = "Mật khẩu hiện tại không đúng.";
            return RedirectToAction("CaiDat");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Đổi mật khẩu", "Admin");

        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction("CaiDat");
    }

    // ── KINH DOANH ────────────────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> KinhDoanhKhachHang(string? q)
    {
        if (!IsSale()) return RedirectToAction("Login", "Account");

        var query = db.Users.Where(u => u.Role == Roles.Customer);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.Name.Contains(q) || u.Email.Contains(q));

        var customers = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

        var orderStats = await db.Orders
            .Where(o => o.CustomerId != null)
            .GroupBy(o => o.CustomerId!.Value)
            .Select(g => new { CustomerId = g.Key, Count = g.Count(), Total = g.Sum(o => (long)o.Amount) })
            .ToListAsync();

        ViewBag.OrderCounts = orderStats.ToDictionary(x => x.CustomerId, x => x.Count);
        ViewBag.OrderTotals = orderStats.ToDictionary(x => x.CustomerId, x => x.Total);
        ViewBag.SearchQ = q;
        return View(customers);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> KinhDoanhDonHang()
    {
        if (!IsSale()) return RedirectToAction("Login", "Account");

        var orders = await db.Orders
            .Where(o => o.OrderType == "buy")
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.TotalOrders    = orders.Count;
        ViewBag.PendingOrders  = orders.Count(o => o.Status == "Chờ xử lý");
        ViewBag.DepositOrders  = orders.Count(o => o.Status == "Đã đặt cọc");
        ViewBag.DoneOrders     = orders.Count(o => o.Status == "Hoàn tất");
        return View(orders);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> KinhDoanhBanGiao()
    {
        if (!IsSale()) return RedirectToAction("Login", "Account");

        var orders = await db.Orders
            .Where(o => o.OrderType == "buy" && o.Status != "Hoàn tất")
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> KinhDoanhDonHangUpdateStatus(Guid id, string status)
    {
        if (!IsSale()) return Forbid();
        var order = await db.Orders.FindAsync(id);
        if (order == null) return RedirectToAction("KinhDoanhDonHang");
        if (!_buyNextStatus.TryGetValue(order.Status, out var allowed) || !allowed.Contains(status)) {
            TempData["Error"] = $"Không thể chuyển trạng thái từ '{order.Status}' sang '{status}'.";
            return RedirectToAction("KinhDoanhDonHang");
        }
        order.Status    = status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("KD - Cập nhật đơn hàng", $"{order.OrderCode} → {status}");
        return RedirectToAction("KinhDoanhDonHang");
    }

    // ── KHO ───────────────────────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> KhoDanhMucXe()
    {
        if (!IsWarehouse()) return RedirectToAction("Login", "Account");
        var cars = await db.Cars.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(cars);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> KhoNhapXuat()
    {
        if (!IsWarehouse()) return RedirectToAction("Login", "Account");

        ViewBag.TongTon      = await db.Cars.SumAsync(c => c.Stock);
        ViewBag.TongApproved = await db.Cars.CountAsync(c => c.Status == "approved");
        ViewBag.TongPending  = await db.Cars.CountAsync(c => c.Status == "pending");

        ViewBag.XuatKho = await db.Orders
            .Where(o => o.OrderType == "buy" && o.Status == "Hoàn tất")
            .OrderByDescending(o => o.UpdatedAt)
            .ToListAsync();

        var cars = await db.Cars.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(cars);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> KhoTonKho()
    {
        if (!IsWarehouse()) return RedirectToAction("Login", "Account");
        var cars = await db.Cars.OrderBy(c => c.Brand).ThenBy(c => c.Name).ToListAsync();
        ViewBag.TongTon = cars.Sum(c => c.Stock);
        return View(cars);
    }

    // ── KẾ TOÁN ───────────────────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanThuChi()
    {
        if (!IsAccounting()) return RedirectToAction("Login", "Account");

        var orders = await db.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.TongThu      = orders.Where(o => o.Status == "Hoàn tất").Sum(o => (long)o.Amount);
        ViewBag.DangXuLy     = orders.Count(o => o.Status == "Đã đặt cọc");
        ViewBag.HoatDong     = orders.Count(o => o.Status != "Hoàn tất" && o.Status != "Đã hủy");
        return View(orders);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanBaoCao()
    {
        if (!IsAccounting()) return RedirectToAction("Login", "Account");

        var ordersByStatus = await db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Revenue = g.Sum(o => (long)o.Amount) })
            .ToListAsync();

        ViewBag.OrdersByStatus = ordersByStatus;
        ViewBag.TotalRevenue   = await db.Orders.Where(o => o.Status == "Hoàn tất").SumAsync(o => (long?)o.Amount) ?? 0;
        ViewBag.TotalOrders    = await db.Orders.CountAsync();
        ViewBag.TopCars        = await db.Orders
            .GroupBy(o => o.CarName)
            .Select(g => new { CarName = g.Key, Count = g.Count(), Total = g.Sum(o => (long)o.Amount) })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToListAsync();

        // Doanh thu theo tháng (12 tháng gần nhất)
        var now = DateTime.UtcNow;
        var revenueByMonth = await db.Orders
            .Where(o => o.Status == "Hoàn tất" && o.UpdatedAt >= now.AddMonths(-12))
            .GroupBy(o => new { o.UpdatedAt.Year, o.UpdatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => (long)o.Amount) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();
        ViewBag.RevenueByMonth = revenueByMonth;

        return View();
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanHoaHong()
    {
        if (!IsAccounting()) return RedirectToAction("Login", "Account");

        // Tính hoa hồng theo nhân viên kinh doanh (StaffId)
        var staff = await db.Users.Where(u => u.Role == Roles.Sale || u.Role == Roles.Manager || u.Role == Roles.Staff).ToListAsync();

        var hoaHong = await db.Orders
            .Where(o => o.StaffId != null && o.Status == "Hoàn tất" && o.OrderType == "buy")
            .GroupBy(o => o.StaffId!.Value)
            .Select(g => new { StaffId = g.Key, SoHopDong = g.Count(), DoanhThu = g.Sum(o => (long)o.Amount) })
            .ToListAsync();

        ViewBag.Staff   = staff;
        ViewBag.HoaHong = hoaHong;
        ViewBag.TiLeHoaHong = 1.5m; // % hoa hồng
        return View();
    }

    // ── KỸ THUẬT & HẬU MÃI ───────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuLichHen()
    {
        if (!IsService()) return RedirectToAction("Login", "Account");
        // Dùng AuditLog để mô phỏng lịch hẹn; thực tế sẽ cần bảng ServiceAppointment
        var logs = await db.AuditLogs
            .Where(l => l.Action.Contains("Bảo dưỡng") || l.Action.Contains("Sửa chữa") || l.Action.Contains("Bảo hành"))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        return View(logs);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuSuaChua(string? q = null, string? status = null)
    {
        if (!IsService()) return RedirectToAction("Login", "Account");
        var query = db.ServiceTickets
            .Include(t => t.AssignedTechnician)
            .Include(t => t.CreatedBy)
            .Include(t => t.SparePartUsages).ThenInclude(u => u.SparePart)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.LicensePlate.Contains(q) || t.CustomerName.Contains(q) || t.TicketCode.Contains(q));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);
        var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        var todayUtc = DateTime.UtcNow.Date;
        ViewBag.DangXuLy     = await db.ServiceTickets.CountAsync(t => t.Status == "assigned" || t.Status == "inprogress");
        ViewBag.HoanTatHomNay = await db.ServiceTickets.CountAsync(t => (t.Status == "completed" || t.Status == "paid") && t.CompletedAt.HasValue && t.CompletedAt.Value >= todayUtc);
        ViewBag.TongLenh     = await db.ServiceTickets.CountAsync();
        ViewBag.DoanhThuDichVu = await db.ServiceTickets.Where(t => t.Status == "paid").SumAsync(t => (long?)t.TotalAmount) ?? 0L;
        ViewBag.CntAll       = await db.ServiceTickets.CountAsync();
        ViewBag.CntReceived  = await db.ServiceTickets.CountAsync(t => t.Status == "received");
        ViewBag.CntAssigned  = await db.ServiceTickets.CountAsync(t => t.Status == "assigned");
        ViewBag.CntInprog    = await db.ServiceTickets.CountAsync(t => t.Status == "inprogress");
        ViewBag.CntCompleted = await db.ServiceTickets.CountAsync(t => t.Status == "completed");
        ViewBag.CntPaid      = await db.ServiceTickets.CountAsync(t => t.Status == "paid");
        ViewBag.Search       = q;
        ViewBag.StatusFilter = status;
        ViewBag.Parts        = await db.SpareParts.Where(s => s.Stock > 0).OrderBy(s => s.Name).ToListAsync();
        return View(tickets);
    }

    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuBaoHanh()
    {
        if (!IsService()) return RedirectToAction("Login", "Account");
        var orders = await db.Orders
            .Where(o => o.OrderType == "buy" && o.Status == "Hoàn tất")
            .Include(o => o.Customer)
            .OrderByDescending(o => o.UpdatedAt)
            .ToListAsync();
        return View(orders);
    }

    // ── QUẢN LÝ NHÂN VIÊN (chỉ Admin) ────────────────────────────────────────

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> NhanSu()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        var users = await db.Users.Where(u => u.Role != Roles.Customer).OrderBy(u => u.Role).ToListAsync();
        return View(users);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> NhanSuAdd(string name, string email, string password, string role, string branch)
    {
        if (!IsAdmin()) return Forbid();

        if (password.Length < 8)
        {
            TempData["Error"] = "Mật khẩu phải có ít nhất 8 ký tự.";
            return RedirectToAction("NhanSu");
        }
        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            TempData["Error"] = "Email này đã được sử dụng.";
            return RedirectToAction("NhanSu");
        }

        db.Users.Add(new User
        {
            Name          = name,
            Email         = email,
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword(password),
            Role          = role,
            Branch        = branch,
            EmailVerified = true
        });
        await db.SaveChangesAsync();
        await LogAsync("Thêm nhân sự", name);

        TempData["Success"] = $"Đã thêm nhân viên {name} ({Roles.DisplayName(role)}).";
        return RedirectToAction("NhanSu");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> NhanSuEdit(Guid id, string name, string role, string branch)
    {
        if (!IsAdmin()) return Forbid();

        var user = await db.Users.FindAsync(id);
        if (user == null || user.Role == Roles.Admin)
        {
            TempData["Error"] = "Không thể sửa tài khoản này.";
            return RedirectToAction("NhanSu");
        }

        user.Name      = name;
        user.Role      = role;
        user.Branch    = branch;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Sửa nhân sự", name);

        TempData["Success"] = $"Đã cập nhật nhân viên {name}.";
        return RedirectToAction("NhanSu");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> NhanSuDelete(Guid id)
    {
        if (!IsAdmin()) return Forbid();
        var user = await db.Users.FindAsync(id);
        if (user != null && user.Role != Roles.Admin)
        {
            await LogAsync("Xóa nhân sự", user.Name);
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
        return RedirectToAction("NhanSu");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> NhanSuToggleLock(Guid id)
    {
        if (!IsAdmin()) return Forbid();
        var user = await db.Users.FindAsync(id);
        if (user != null && user.Role != Roles.Admin)
        {
            user.IsActive  = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            var action = user.IsActive ? "Mở khóa tài khoản" : "Khóa tài khoản";
            await LogAsync(action, user.Name);
            TempData["Success"] = $"{action} {user.Name} thành công.";
        }
        return RedirectToAction("NhanSu");
    }

    // ── KINH DOANH — Todo List ────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> TodoList()
    {
        if (!IsSale()) return RedirectToAction("Login", "Account");
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return RedirectToAction("Login", "Account");
        var todos = await db.TodoItems.Where(t => t.AssignedToId == uid)
            .OrderBy(t => t.DueDate).ThenByDescending(t => t.CreatedAt).ToListAsync();
        return View(todos);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> TodoAdd(string title, string? description, DateTime? dueDate)
    {
        if (!IsSale()) return Forbid();
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        db.TodoItems.Add(new TodoItem { Title = title, Description = description, DueDate = dueDate, AssignedToId = uid });
        await db.SaveChangesAsync();
        return RedirectToAction("TodoList");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> TodoUpdateStatus(Guid id, string status)
    {
        if (!IsSale()) return Forbid();
        var item = await db.TodoItems.FindAsync(id);
        if (item != null) { item.Status = status; item.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(); }
        return RedirectToAction("TodoList");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> TodoDelete(Guid id)
    {
        if (!IsSale()) return Forbid();
        var item = await db.TodoItems.FindAsync(id);
        if (item != null) { db.TodoItems.Remove(item); await db.SaveChangesAsync(); }
        return RedirectToAction("TodoList");
    }

    // ── KINH DOANH — CRM ─────────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> KinhDoanhCRM(string? q)
    {
        if (!IsSale()) return RedirectToAction("Login", "Account");
        var query = db.Users.Where(u => u.Role == Roles.Customer);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.Name.Contains(q) || u.Email.Contains(q));
        var customers = await query.OrderByDescending(u => u.CreatedAt).Take(50).ToListAsync();
        var ids = customers.Select(c => c.Id).ToList();
        ViewBag.Profiles = await db.CustomerProfiles.Where(p => ids.Contains(p.CustomerId)).ToDictionaryAsync(p => p.CustomerId);
        var notes = await db.CustomerNotes.Where(n => ids.Contains(n.CustomerId)).Include(n => n.CreatedBy).OrderByDescending(n => n.CreatedAt).ToListAsync();
        ViewBag.NotesByCustomer = notes.GroupBy(n => n.CustomerId).ToDictionary(g => g.Key, g => g.ToList());
        ViewBag.SearchQ = q;
        ViewBag.AllCars = await db.Cars.Where(c => c.Status == "approved").Select(c => c.Name).Distinct().ToListAsync();
        return View(customers);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> KinhDoanhCRMSave(Guid customerId, string source, string? interestedCarModel, string? phone, string? summary)
    {
        if (!IsSale()) return Forbid();
        var profile = await db.CustomerProfiles.FirstOrDefaultAsync(p => p.CustomerId == customerId);
        if (profile == null)
            db.CustomerProfiles.Add(new CustomerProfile { CustomerId = customerId, Source = source, InterestedCarModel = interestedCarModel, Phone = phone, Summary = summary });
        else
        { profile.Source = source; profile.InterestedCarModel = interestedCarModel; profile.Phone = phone; profile.Summary = summary; profile.UpdatedAt = DateTime.UtcNow; }
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật hồ sơ khách hàng.";
        return RedirectToAction("KinhDoanhCRM");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> KinhDoanhNoteAdd(Guid customerId, string content)
    {
        if (!IsSale()) return Forbid();
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        db.CustomerNotes.Add(new CustomerNote { CustomerId = customerId, Content = content, CreatedById = uid });
        await db.SaveChangesAsync();
        return RedirectToAction("KinhDoanhCRM");
    }

    // ── KINH DOANH — Lịch lái thử ────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> LaiThu()
    {
        if (!IsSale()) return RedirectToAction("Login", "Account");
        var drives = await db.TestDrives.Include(t => t.Customer).Include(t => t.Car).OrderByDescending(t => t.ScheduledDate).ToListAsync();
        ViewBag.Customers = await db.Users.Where(u => u.Role == Roles.Customer).OrderBy(u => u.Name).ToListAsync();
        ViewBag.Cars = await db.Cars.Where(c => c.Status == "approved").OrderBy(c => c.Name).ToListAsync();
        return View(drives);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> LaiThuAdd(Guid customerId, Guid carId, string licensePlate, DateTime scheduledDate, string startTime, string endTime, string? notes)
    {
        if (!IsSale()) return Forbid();
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        var start = TimeSpan.Parse(startTime);
        var end   = TimeSpan.Parse(endTime);
        if (end <= start) { TempData["Error"] = "Giờ kết thúc phải sau giờ bắt đầu."; return RedirectToAction("LaiThu"); }
        bool conflict = await db.TestDrives.AnyAsync(t =>
            t.LicensePlate == licensePlate.ToUpper() && t.ScheduledDate.Date == scheduledDate.Date &&
            t.Status != "cancelled" && t.StartTime < end && t.EndTime > start);
        if (conflict) { TempData["Error"] = $"Xe {licensePlate} đã có lịch lái thử trong khung giờ này!"; return RedirectToAction("LaiThu"); }
        db.TestDrives.Add(new TestDrive { CustomerId = customerId, CarId = carId, LicensePlate = licensePlate.Trim().ToUpper(), ScheduledDate = scheduledDate, StartTime = start, EndTime = end, Notes = notes, CreatedById = uid });
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã đặt lịch lái thử thành công.";
        return RedirectToAction("LaiThu");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> LaiThuUpdateStatus(Guid id, string status)
    {
        if (!IsSale()) return Forbid();
        var drive = await db.TestDrives.FindAsync(id);
        if (drive != null) { drive.Status = status; drive.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(); }
        return RedirectToAction("LaiThu");
    }

    // ── KINH DOANH — Tính trả góp ────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public IActionResult TraGop()
    {
        if (!IsSale()) return RedirectToAction("Login", "Account");
        return View();
    }

    // ── KINH DOANH — Tạo đơn đặt cọc ────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> TaoDonHang()
    {
        if (!IsSale()) return RedirectToAction("Login", "Account");
        ViewBag.Customers = await db.Users.Where(u => u.Role == Roles.Customer).OrderBy(u => u.Name).ToListAsync();
        ViewBag.AvailableVehicles = await db.VehicleUnits.Where(v => v.Status == "available").Include(v => v.Car).OrderBy(v => v.Car.Name).ToListAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Sale)]
    public async Task<IActionResult> TaoDonHangSave(Guid customerId, Guid vehicleUnitId, long depositAmount, string? notes)
    {
        if (!IsSale()) return Forbid();
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        var vehicle  = await db.VehicleUnits.Include(v => v.Car).FirstOrDefaultAsync(v => v.Id == vehicleUnitId);
        var customer = await db.Users.FindAsync(customerId);
        if (vehicle == null || customer == null) { TempData["Error"] = "Thông tin không hợp lệ."; return RedirectToAction("TaoDonHang"); }
        if (vehicle.Status != "available") { TempData["Error"] = "Xe này đã được giữ chỗ hoặc đã bán."; return RedirectToAction("TaoDonHang"); }
        var code = $"HD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        while (await db.Orders.AnyAsync(o => o.OrderCode == code))
            code = $"HD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        db.Orders.Add(new Order { OrderCode = code, CustomerId = customerId, CustomerName = customer.Name, CarId = vehicle.CarId, VehicleUnitId = vehicleUnitId, CarName = vehicle.Car.Name, OrderType = "buy", Amount = vehicle.ListPrice > 0 ? vehicle.ListPrice : vehicle.Car.Price, DepositAmount = depositAmount, Status = "Chờ kế toán duyệt tiền", StaffId = uid, Notes = notes });
        vehicle.Status = "reserved"; vehicle.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Tạo đơn đặt cọc", code, $"{vehicle.Car.Name} — Cọc: {depositAmount:N0} ₫");
        TempData["Success"] = $"Đã tạo đơn {code}. Xe chuyển sang 'Đã giữ chỗ'.";
        return RedirectToAction("KinhDoanhDonHang");
    }

    // ── KHO — Xe cụ thể (VehicleUnit) ────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> KhoXeUnit()
    {
        if (!IsWarehouse()) return RedirectToAction("Login", "Account");
        var units = await db.VehicleUnits.Include(v => v.Car).Include(v => v.CreatedBy).OrderByDescending(v => v.CreatedAt).ToListAsync();
        ViewBag.Cars = await db.Cars.Where(c => c.Status == "approved").OrderBy(c => c.Name).ToListAsync();
        var unitIds  = units.Select(u => u.Id).ToList();
        var pdiList  = await db.PdiChecklists.Where(p => unitIds.Contains(p.VehicleUnitId)).OrderByDescending(p => p.InspectedAt).ToListAsync();
        ViewBag.PdiMap = pdiList.GroupBy(p => p.VehicleUnitId).ToDictionary(g => g.Key, g => g.First());
        return View(units);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> KhoXeUnitAdd(string vin, string engineNumber, Guid carId, string exteriorColor, string interiorColor, long purchasePrice, long listPrice)
    {
        if (!IsWarehouse()) return Forbid();
        if (await db.VehicleUnits.AnyAsync(v => v.Vin == vin.Trim().ToUpper())) { TempData["Error"] = $"VIN '{vin}' đã tồn tại."; return RedirectToAction("KhoXeUnit"); }
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        db.VehicleUnits.Add(new VehicleUnit { Vin = vin.Trim().ToUpper(), EngineNumber = engineNumber.Trim().ToUpper(), CarId = carId, ExteriorColor = exteriorColor, InteriorColor = interiorColor, PurchasePrice = purchasePrice, ListPrice = listPrice, Status = "available", CreatedById = uid });
        await db.SaveChangesAsync();
        await LogAsync("Nhập kho xe", vin.ToUpper());
        TempData["Success"] = $"Đã nhập xe VIN {vin.ToUpper()} vào kho.";
        return RedirectToAction("KhoXeUnit");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> KhoXeUnitEdit(Guid id, string exteriorColor, string interiorColor, long purchasePrice, long listPrice, string status)
    {
        if (!IsWarehouse()) return Forbid();
        var unit = await db.VehicleUnits.FindAsync(id);
        if (unit == null) { TempData["Error"] = "Không tìm thấy xe."; return RedirectToAction("KhoXeUnit"); }
        unit.ExteriorColor = exteriorColor; unit.InteriorColor = interiorColor;
        unit.PurchasePrice = purchasePrice; unit.ListPrice = listPrice;
        unit.Status = status; unit.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Cập nhật xe kho", unit.Vin);
        TempData["Success"] = "Đã cập nhật thông tin xe.";
        return RedirectToAction("KhoXeUnit");
    }

    // ── KHO — PDI Kiểm định ───────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> PdiCreate(Guid vehicleUnitId)
    {
        if (!IsWarehouse()) return RedirectToAction("Login", "Account");
        var unit = await db.VehicleUnits.Include(v => v.Car).FirstOrDefaultAsync(v => v.Id == vehicleUnitId);
        if (unit == null) return NotFound();
        return View(unit);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> PdiSave(Guid vehicleUnitId, bool exteriorPassed, bool interiorPassed, bool electricalPassed, bool enginePassed, bool tirePassed, string? notes, List<string>? defectCategories, List<string>? defectReasons, List<IFormFile>? defectPhotos)
    {
        if (!IsWarehouse()) return Forbid();
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        var pdi = new PdiChecklist { VehicleUnitId = vehicleUnitId, ExteriorPassed = exteriorPassed, InteriorPassed = interiorPassed, ElectricalPassed = electricalPassed, EnginePassed = enginePassed, TirePassed = tirePassed, Notes = notes, InspectorId = uid };
        db.PdiChecklists.Add(pdi);
        bool hasFailed = !exteriorPassed || !interiorPassed || !electricalPassed || !enginePassed || !tirePassed;
        if (hasFailed && defectCategories != null)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pdi");
            Directory.CreateDirectory(dir);
            for (int i = 0; i < defectCategories.Count; i++)
            {
                string? photoPath = null;
                if (defectPhotos != null && i < defectPhotos.Count && defectPhotos[i]?.Length > 0)
                {
                    var ext = Path.GetExtension(defectPhotos[i].FileName).ToLowerInvariant();
                    if (ext is ".jpg" or ".jpeg" or ".png" or ".webp")
                    {
                        var fn = $"pdi_{Guid.NewGuid():N}{ext}";
                        using var st = new FileStream(Path.Combine(dir, fn), FileMode.Create);
                        await defectPhotos[i].CopyToAsync(st);
                        photoPath = $"/uploads/pdi/{fn}";
                    }
                }
                pdi.Defects.Add(new PdiDefect { Category = defectCategories[i], Reason = defectReasons != null && i < defectReasons.Count ? defectReasons[i] : "", PhotoPath = photoPath });
            }
        }
        await db.SaveChangesAsync();
        var vehicle = await db.VehicleUnits.FindAsync(vehicleUnitId);
        if (vehicle != null && hasFailed) { vehicle.Status = "repair"; vehicle.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(); }
        await LogAsync("PDI kiểm định", vehicle?.Vin, hasFailed ? "Có hạng mục không đạt → Chờ sửa" : "Đạt tất cả");
        TempData["Success"] = hasFailed ? "PDI xong — Xe chuyển sang 'Chờ sửa chữa'." : "PDI xong — Tất cả hạng mục đạt yêu cầu.";
        return RedirectToAction("KhoXeUnit");
    }

    // ── KHO — Chuẩn bị xe giao ───────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> ChuanBiXe()
    {
        if (!IsWarehouse()) return RedirectToAction("Login", "Account");
        var pdos = await db.PreDeliveryOrders.Include(p => p.Order).ThenInclude(o => o.Customer).Include(p => p.VehicleUnit).ThenInclude(v => v.Car).Include(p => p.AssignedTo).OrderByDescending(p => p.CreatedAt).ToListAsync();
        var paidOrderIds = await db.PaymentReceipts.Where(r => r.PaymentType == "full").Select(r => r.OrderId).Distinct().ToListAsync();
        ViewBag.ReadyOrders = await db.Orders.Where(o => o.VehicleUnitId != null && paidOrderIds.Contains(o.Id) && !db.PreDeliveryOrders.Any(p => p.OrderId == o.Id)).Include(o => o.Customer).ToListAsync();
        ViewBag.Staff = await db.Users.Where(u => u.Role == Roles.Warehouse || u.Role == Roles.Admin).ToListAsync();
        return View(pdos);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> ChuanBiXeCreate(Guid orderId, Guid? assignedToId, string? instructions)
    {
        if (!IsWarehouse()) return Forbid();
        var order = await db.Orders.FindAsync(orderId);
        if (order?.VehicleUnitId == null) { TempData["Error"] = "Đơn hàng không hợp lệ."; return RedirectToAction("ChuanBiXe"); }
        db.PreDeliveryOrders.Add(new PreDeliveryOrder { OrderId = orderId, VehicleUnitId = order.VehicleUnitId.Value, InstructionsJson = instructions ?? "[]", AssignedToId = assignedToId });
        await db.SaveChangesAsync();
        await LogAsync("Tạo lệnh chuẩn bị xe", order.OrderCode);
        TempData["Success"] = "Đã tạo lệnh chuẩn bị xe giao.";
        return RedirectToAction("ChuanBiXe");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> ChuanBiXeUpdateStatus(Guid id, string status)
    {
        if (!IsWarehouse()) return Forbid();
        var pdo = await db.PreDeliveryOrders.FindAsync(id);
        if (pdo != null) { pdo.Status = status; if (status == "done") pdo.CompletedAt = DateTime.UtcNow; await db.SaveChangesAsync(); }
        return RedirectToAction("ChuanBiXe");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Warehouse)]
    public async Task<IActionResult> XacNhanXuatKho(Guid orderId)
    {
        if (!IsWarehouse()) return Forbid();
        var order = await db.Orders.FindAsync(orderId);
        if (order?.VehicleUnitId == null) { TempData["Error"] = "Đơn hàng không hợp lệ."; return RedirectToAction("ChuanBiXe"); }
        var vehicle = await db.VehicleUnits.FindAsync(order.VehicleUnitId.Value);
        if (vehicle != null) { vehicle.Status = "sold"; vehicle.UpdatedAt = DateTime.UtcNow; }
        order.Status = "Hoàn tất"; order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Xuất kho — Giao xe", order.OrderCode);
        TempData["Success"] = $"Đã xác nhận xuất kho {order.OrderCode}. Xe chuyển sang 'Đã bán'.";
        return RedirectToAction("ChuanBiXe");
    }

    // ── KẾ TOÁN — Duyệt đơn & Thu tiền ──────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanDuyetDon()
    {
        if (!IsAccounting()) return RedirectToAction("Login", "Account");
        var orders = await db.Orders.Where(o => o.Status == "Chờ kế toán duyệt tiền" || o.Status == "Đã đặt cọc — Chờ thanh toán đủ").Include(o => o.Customer).OrderByDescending(o => o.CreatedAt).ToListAsync();
        var ids = orders.Select(o => o.Id).ToList();
        var receipts = await db.PaymentReceipts.Where(r => ids.Contains(r.OrderId)).ToListAsync();
        ViewBag.ReceiptsByOrder = receipts.GroupBy(r => r.OrderId).ToDictionary(g => g.Key, g => g.ToList());
        return View(orders);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanXacNhanThu(Guid orderId, string paymentType, long amount, string? notes)
    {
        if (!IsAccounting()) return Forbid();
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        var order = await db.Orders.FindAsync(orderId);
        if (order == null) { TempData["Error"] = "Không tìm thấy đơn hàng."; return RedirectToAction("KeToanDuyetDon"); }
        var code = $"PT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        while (await db.PaymentReceipts.AnyAsync(r => r.ReceiptCode == code))
            code = $"PT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        db.PaymentReceipts.Add(new PaymentReceipt { ReceiptCode = code, OrderId = orderId, Amount = amount, PaymentType = paymentType, ConfirmedById = uid, Notes = notes });
        order.Status = paymentType == "full" ? "Chờ bàn giao xe" : "Đã đặt cọc — Chờ thanh toán đủ";
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Xác nhận thu tiền", $"{order.OrderCode} ({paymentType})", $"{amount:N0} ₫");
        TempData["Success"] = $"Đã tạo phiếu thu {code}.";
        return RedirectToAction("KeToanDuyetDon");
    }

    // ── KẾ TOÁN — Công nợ ngân hàng ──────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanCongNoBanking()
    {
        if (!IsAccounting()) return RedirectToAction("Login", "Account");
        var loans = await db.BankLoans.Include(b => b.Order).ThenInclude(o => o!.Customer).Include(b => b.ConfirmedBy).OrderByDescending(b => b.CreatedAt).ToListAsync();
        ViewBag.EligibleOrders = await db.Orders.Where(o => o.Status == "Đã đặt cọc — Chờ thanh toán đủ" && !db.BankLoans.Any(b => b.OrderId == o.Id)).Include(o => o.Customer).ToListAsync();
        return View(loans);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanCongNoAdd(Guid orderId, string bankName, long loanAmount, decimal interestRate, int loanYears, string? notes)
    {
        if (!IsAccounting()) return Forbid();
        db.BankLoans.Add(new BankLoan { OrderId = orderId, BankName = bankName, LoanAmount = loanAmount, InterestRate = interestRate, LoanYears = loanYears, Notes = notes });
        await db.SaveChangesAsync();
        await LogAsync("Thêm hồ sơ vay NH", bankName);
        TempData["Success"] = "Đã thêm hồ sơ vay ngân hàng.";
        return RedirectToAction("KeToanCongNoBanking");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanGiaiNgan(Guid loanId, string? notes)
    {
        if (!IsAccounting()) return Forbid();
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        var loan = await db.BankLoans.Include(b => b.Order).FirstOrDefaultAsync(b => b.Id == loanId);
        if (loan == null) { TempData["Error"] = "Không tìm thấy hồ sơ."; return RedirectToAction("KeToanCongNoBanking"); }
        loan.Status = "disbursed"; loan.DisbursedAt = DateTime.UtcNow; loan.ConfirmedById = uid; loan.Notes = notes ?? loan.Notes;
        var code = $"PT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        while (await db.PaymentReceipts.AnyAsync(r => r.ReceiptCode == code))
            code = $"PT-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        db.PaymentReceipts.Add(new PaymentReceipt { ReceiptCode = code, OrderId = loan.OrderId, Amount = loan.LoanAmount, PaymentType = "full", ConfirmedById = uid, Notes = $"Giải ngân từ {loan.BankName}" });
        if (loan.Order != null) { loan.Order.Status = "Chờ bàn giao xe"; loan.Order.UpdatedAt = DateTime.UtcNow; }
        await db.SaveChangesAsync();
        await LogAsync("Xác nhận giải ngân", loan.BankName, $"{loan.LoanAmount:N0} ₫");
        TempData["Success"] = $"Đã xác nhận giải ngân. Phiếu thu {code} tạo tự động.";
        return RedirectToAction("KeToanCongNoBanking");
    }

    // ── KẾ TOÁN — Phiếu chi ──────────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanPhieuChi()
    {
        if (!IsAccounting()) return RedirectToAction("Login", "Account");
        var vouchers = await db.PaymentVouchers.Include(v => v.CreatedBy).OrderByDescending(v => v.CreatedAt).ToListAsync();
        ViewBag.TongChi = vouchers.Sum(v => v.Amount);
        return View(vouchers);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanPhieuChiAdd(string category, long amount, string recipient, string description)
    {
        if (!IsAccounting()) return Forbid();
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        var code = $"PC-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        while (await db.PaymentVouchers.AnyAsync(v => v.VoucherCode == code))
            code = $"PC-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        db.PaymentVouchers.Add(new PaymentVoucher { VoucherCode = code, Category = category, Amount = amount, Recipient = recipient, Description = description, CreatedById = uid });
        await db.SaveChangesAsync();
        await LogAsync("Tạo phiếu chi", code, $"{recipient} — {amount:N0} ₫");
        TempData["Success"] = $"Đã tạo phiếu chi {code}.";
        return RedirectToAction("KeToanPhieuChi");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Accounting)]
    public async Task<IActionResult> KeToanTinhHoaHong(int month, int year)
    {
        if (!IsAccounting()) return Forbid();
        const decimal rate = 0.01m;
        var orders = await db.Orders.Where(o => o.StaffId != null && o.Status == "Hoàn tất" && o.OrderType == "buy" && o.UpdatedAt.Month == month && o.UpdatedAt.Year == year).ToListAsync();
        int added = 0;
        foreach (var o in orders)
        {
            if (!await db.Commissions.AnyAsync(c => c.OrderId == o.Id))
            {
                db.Commissions.Add(new Commission { StaffId = o.StaffId!.Value, OrderId = o.Id, Rate = rate, Amount = (long)(o.Amount * rate), Month = month, Year = year });
                added++;
            }
        }
        await db.SaveChangesAsync();
        await LogAsync("Tính hoa hồng", $"Tháng {month}/{year}", $"{added} hoa hồng mới");
        TempData["Success"] = $"Đã tính {added} hoa hồng mới cho tháng {month}/{year}.";
        return RedirectToAction("KeToanHoaHong");
    }

    // ── KỸ THUẬT — Phiếu dịch vụ ─────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuPhieuDichVu()
    {
        if (!IsService()) return RedirectToAction("Login", "Account");
        var tickets = await db.ServiceTickets.Include(t => t.AssignedTechnician).Include(t => t.CreatedBy).OrderByDescending(t => t.CreatedAt).ToListAsync();
        ViewBag.Technicians = await db.Users.Where(u => u.Role == Roles.Service).ToListAsync();
        var ticketIds = tickets.Select(t => t.Id).ToList();
        ViewBag.UsagesByTicket = (await db.SparePartUsages.Where(u => ticketIds.Contains(u.ServiceTicketId)).Include(u => u.SparePart).ToListAsync()).GroupBy(u => u.ServiceTicketId).ToDictionary(g => g.Key, g => g.ToList());
        ViewBag.Parts = await db.SpareParts.Where(s => s.Stock > 0).OrderBy(s => s.Name).ToListAsync();
        ViewBag.TongPhieu   = tickets.Count;
        ViewBag.ChoPhanCong = tickets.Count(t => t.Status == "received");
        ViewBag.DangSua     = tickets.Count(t => t.Status is "assigned" or "inprogress");
        ViewBag.HoanThanh   = tickets.Count(t => t.Status is "completed" or "paid");
        return View(tickets);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuPhieuAdd(string licensePlate, string customerName, string customerPhone, int odometer, string description)
    {
        if (!IsService()) return Forbid();
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid)) return Forbid();
        var code = $"DV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        while (await db.ServiceTickets.AnyAsync(t => t.TicketCode == code))
            code = $"DV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        db.ServiceTickets.Add(new ServiceTicket { TicketCode = code, LicensePlate = licensePlate.Trim().ToUpper(), CustomerName = customerName, CustomerPhone = customerPhone, Odometer = odometer, Description = description, CreatedById = uid });
        await db.SaveChangesAsync();
        await LogAsync("Tiếp nhận xe dịch vụ", $"{code} — {licensePlate}");
        TempData["Success"] = $"Đã tạo phiếu dịch vụ {code}.";
        return RedirectToAction("DichVuPhieuDichVu");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuPhanCong(Guid ticketId, Guid technicianId)
    {
        if (!IsService()) return Forbid();
        var ticket = await db.ServiceTickets.FindAsync(ticketId);
        if (ticket != null) { ticket.AssignedTechnicianId = technicianId; ticket.Status = "assigned"; await db.SaveChangesAsync(); var tech = await db.Users.FindAsync(technicianId); await LogAsync("Phân công KTV", ticket.TicketCode, tech?.Name); }
        TempData["Success"] = "Đã phân công kỹ thuật viên.";
        return RedirectToAction("DichVuPhieuDichVu");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuPhuTungSuDung(Guid ticketId, Guid sparePartId, int quantity, string? returnTo = null)
    {
        if (!IsService()) return Forbid();
        var part = await db.SpareParts.FindAsync(sparePartId);
        var ret = returnTo == "suachua" ? "DichVuSuaChua" : "DichVuPhieuDichVu";
        if (part == null || part.Stock < quantity) { TempData["Error"] = part == null ? "Không tìm thấy phụ tùng." : $"Tồn kho không đủ (còn {part.Stock} {part.Unit})."; return RedirectToAction(ret); }
        db.SparePartUsages.Add(new SparePartUsage { ServiceTicketId = ticketId, SparePartId = sparePartId, Quantity = quantity, UnitPrice = part.UnitPrice });
        part.Stock -= quantity; part.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var ticket = await db.ServiceTickets.Include(t => t.SparePartUsages).FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket != null) { ticket.TotalPartsCost = ticket.SparePartUsages.Sum(u => u.Quantity * u.UnitPrice); ticket.TotalAmount = ticket.LaborCost + ticket.TotalPartsCost; ticket.Status = "inprogress"; await db.SaveChangesAsync(); }
        TempData[part.IsLowStock ? "Warning" : "Success"] = part.IsLowStock ? $"Cảnh báo: '{part.Name}' còn {part.Stock} {part.Unit} — dưới mức tối thiểu!" : $"Đã ghi nhận sử dụng {quantity} {part.Unit} {part.Name}.";
        return RedirectToAction(ret);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuHoanThanh(Guid ticketId, long laborCost, string? returnTo = null)
    {
        if (!IsService()) return Forbid();
        var ret = returnTo == "suachua" ? "DichVuSuaChua" : "DichVuPhieuDichVu";
        var ticket = await db.ServiceTickets.Include(t => t.SparePartUsages).FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null) { TempData["Error"] = "Không tìm thấy phiếu."; return RedirectToAction(ret); }
        ticket.LaborCost = laborCost; ticket.TotalPartsCost = ticket.SparePartUsages.Sum(u => u.Quantity * u.UnitPrice); ticket.TotalAmount = ticket.LaborCost + ticket.TotalPartsCost; ticket.Status = "completed"; ticket.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await LogAsync("Hoàn thành sửa chữa", ticket.TicketCode, $"Tổng: {ticket.TotalAmount:N0} ₫");
        TempData["Success"] = $"Phiếu {ticket.TicketCode} hoàn thành — {ticket.TotalAmount:N0} ₫.";
        return RedirectToAction(ret);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuThanhToan(Guid ticketId, string? returnTo = null)
    {
        if (!IsService()) return Forbid();
        var ret = returnTo == "suachua" ? "DichVuSuaChua" : "DichVuPhieuDichVu";
        var ticket = await db.ServiceTickets.FindAsync(ticketId);
        if (ticket != null) { ticket.Status = "paid"; await db.SaveChangesAsync(); await LogAsync("Thu tiền dịch vụ", ticket.TicketCode); }
        TempData["Success"] = "Đã xác nhận thanh toán. Xe được phép xuất xưởng.";
        return RedirectToAction(ret);
    }

    // ── KỸ THUẬT — Kho phụ tùng ──────────────────────────────────────────────

    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuKhoPhuTung()
    {
        if (!IsService()) return RedirectToAction("Login", "Account");
        var parts = await db.SpareParts.OrderBy(s => s.Name).ToListAsync();
        ViewBag.LowStockCount = parts.Count(s => s.IsLowStock);
        return View(parts);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuPhuTungAdd(string partCode, string name, int stock, int minStock, long unitPrice, string unit)
    {
        if (!IsService()) return Forbid();
        if (await db.SpareParts.AnyAsync(s => s.PartCode == partCode.Trim().ToUpper())) { TempData["Error"] = $"Mã '{partCode}' đã tồn tại."; return RedirectToAction("DichVuKhoPhuTung"); }
        db.SpareParts.Add(new SparePart { PartCode = partCode.Trim().ToUpper(), Name = name, Stock = stock, MinStock = minStock, UnitPrice = unitPrice, Unit = unit });
        await db.SaveChangesAsync();
        await LogAsync("Thêm phụ tùng", partCode);
        TempData["Success"] = $"Đã thêm phụ tùng {name}.";
        return RedirectToAction("DichVuKhoPhuTung");
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin + "," + Roles.Service)]
    public async Task<IActionResult> DichVuPhuTungEdit(Guid id, string name, int stock, int minStock, long unitPrice, string unit)
    {
        if (!IsService()) return Forbid();
        var part = await db.SpareParts.FindAsync(id);
        if (part != null) { part.Name = name; part.Stock = stock; part.MinStock = minStock; part.UnitPrice = unitPrice; part.Unit = unit; part.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(); await LogAsync("Cập nhật phụ tùng", part.PartCode); }
        TempData["Success"] = "Đã cập nhật phụ tùng.";
        return RedirectToAction("DichVuKhoPhuTung");
    }

    private async Task LogAsync(string action, string? target = null, string? detail = null)
    {
        db.AuditLogs.Add(new AuditLog
        {
            UserId = Guid.TryParse(HttpContext.Session.GetString("UserId"), out var uid) ? uid : null,
            UserName = HttpContext.Session.GetString("UserName") ?? "System",
            Action = action,
            Target = target,
            Detail = detail
        });
        await db.SaveChangesAsync();
    }
}
