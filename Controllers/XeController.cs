using CarWebsite.Data;
using CarWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

public class XeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? brand, string? type, string? fuel, string? q, long? minPrice, long? maxPrice)
    {
        var query = db.Cars.Where(c => c.Status == "approved").AsQueryable();

        if (!string.IsNullOrWhiteSpace(brand) && brand != "Tất cả")
            query = query.Where(c => c.Brand == brand);
        if (!string.IsNullOrWhiteSpace(type) && type != "Tất cả")
            query = query.Where(c => c.Type == type);
        if (!string.IsNullOrWhiteSpace(fuel) && fuel != "Tất cả")
            query = query.Where(c => c.Fuel == fuel);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Name.Contains(q) || c.Brand.Contains(q));
        if (minPrice.HasValue)
            query = query.Where(c => c.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(c => c.Price <= maxPrice.Value);

        var cars = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

        ViewBag.AllBrands = new[] { "Tất cả", "Mercedes-Benz", "BMW", "Audi", "Toyota", "Honda", "VinFast" };
        ViewBag.AllTypes = new[] { "Tất cả", "Sedan", "SUV", "Coupe", "Pickup", "Hatchback", "Van" };
        ViewBag.AllFuels = new[] { "Tất cả", "Xăng", "Điện", "Hybrid" };
        ViewBag.SelectedBrand = brand ?? "Tất cả";
        ViewBag.SelectedType = type ?? "Tất cả";
        ViewBag.SelectedFuel = fuel ?? "Tất cả";
        ViewBag.SearchQ = q;

        return View(cars);
    }

    [HttpGet]
    public async Task<IActionResult> SearchSuggest(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(Array.Empty<object>());

        var suggestions = await db.Cars
            .Where(c => c.Status == "approved" && (c.Name.Contains(q) || c.Brand.Contains(q)))
            .OrderBy(c => c.Name)
            .Take(6)
            .Select(c => new { c.Name, c.Brand, c.Slug })
            .ToListAsync();

        return Json(suggestions);
    }

    public async Task<IActionResult> Detail(string slug)
    {
        var car = await db.Cars.FirstOrDefaultAsync(c => c.Slug == slug && c.Status == "approved");
        if (car == null) return NotFound();

        var related = await db.Cars
            .Where(c => c.Type == car.Type && c.Id != car.Id && c.Status == "approved")
            .Take(3)
            .ToListAsync();

        ViewBag.RelatedCars = related;
        return View(car);
    }

    public async Task<IActionResult> Viewer3D(string slug)
    {
        var car = await db.Cars.FirstOrDefaultAsync(c => c.Slug == slug && c.Status == "approved");
        if (car == null || string.IsNullOrEmpty(car.Model3DUrl)) return NotFound();
        return View(car);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DatMua(Guid carId, string orderType, DateTime? rentalStartDate, DateTime? rentalEndDate)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, requireLogin = true });

        if (!Guid.TryParse(userId, out var uid))
            return Json(new { success = false, error = "Phiên đăng nhập không hợp lệ." });

        var car = await db.Cars.FindAsync(carId);
        if (car == null || car.Status != "approved")
            return Json(new { success = false, error = "Xe không tồn tại." });

        var user = await db.Users.FindAsync(uid);
        if (user == null)
            return Json(new { success = false, error = "Không tìm thấy tài khoản." });

        var role = HttpContext.Session.GetString("UserRole");
        if (role is "admin" or "manager" or "staff")
            return Json(new { success = false, error = "Nhân viên không thể đặt hàng. Vui lòng dùng tài khoản khách hàng." });

        bool isRent = orderType == "rent";
        var order = new Order
        {
            OrderCode = $"AE{DateTime.UtcNow:yyMMddHHmmss}{new Random().Next(100, 999)}",
            CustomerId = uid,
            CarId = car.Id,
            CarName = car.Name,
            CustomerName = user.Name,
            OrderType = isRent ? "rent" : "buy",
            Amount = isRent ? 0 : car.Price,
            RentalDailyRate = isRent ? car.RentalPricePerDay : 0,
            RentalStartDate = isRent ? rentalStartDate : null,
            RentalEndDate = isRent ? rentalEndDate : null,
            Status = isRent ? "Đang đặt thuê" : "Khách đang xem xe"
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        string msg = orderType == "rent"
            ? $"Đặt thuê {car.Name} thành công!"
            : $"Đặt mua {car.Name} thành công!";

        return Json(new { success = true, message = msg, orderCode = order.OrderCode });
    }
}
