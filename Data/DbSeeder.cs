using CarWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Mật khẩu chung cho tất cả nhân viên
        const string staffPassword = "123456";
        var staffHash = BCrypt.Net.BCrypt.HashPassword(staffPassword);

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new User
                {
                    Email = "admin@autoht.vn",
                    Name = "Admin AutoHT",
                    Role = Roles.Admin,
                    Branch = "Tất cả",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Avatar = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&q=80",
                    EmailVerified = true
                },
                new User { Email = "kinhdoanh@gmail.com", Name = "Kinh Doanh",   Role = Roles.Sale,       Branch = "TP. Hồ Chí Minh", PasswordHash = staffHash, EmailVerified = true },
                new User { Email = "kho@gmail.com",       Name = "Nhân Viên Kho", Role = Roles.Warehouse,  Branch = "TP. Hồ Chí Minh", PasswordHash = staffHash, EmailVerified = true },
                new User { Email = "ketoan@gmail.com",    Name = "Kế Toán",      Role = Roles.Accounting, Branch = "TP. Hồ Chí Minh", PasswordHash = staffHash, EmailVerified = true },
                new User { Email = "kythuat@gmail.com",   Name = "Kỹ Thuật",     Role = Roles.Service,    Branch = "TP. Hồ Chí Minh", PasswordHash = staffHash, EmailVerified = true }
            );
            await db.SaveChangesAsync();
        }

        // Cập nhật email/tên/mật khẩu cho tất cả nhân viên (trừ admin) về chuẩn mới
        await SyncStaffAccountsAsync(db, staffHash);

        // Tạo 20 khách hàng test nếu chưa có
        await SeedTestCustomersAsync(db);

        // Tạo dữ liệu test đầy đủ cho 20 khách hàng
        await SeedTestScenarioAsync(db);

        var existingSlugs = await db.Cars.Select(c => c.Slug).ToHashSetAsync();
        var newCars = AllCars().Where(c => !existingSlugs.Contains(c.Slug)).ToList();
        if (newCars.Any())
        {
            db.Cars.AddRange(newCars);
            await db.SaveChangesAsync();
        }

        if (!await db.News.AnyAsync())
        {
            db.News.AddRange(
                new News
                {
                    Slug = "xu-huong-xe-dien-2024",
                    Title = "Xu hÆ°á»›ng xe Ä‘iá»‡n 2024: Cuá»™c cÃ¡ch máº¡ng trÃªn Ä‘Æ°á»ng phá»‘ Viá»‡t Nam",
                    Excerpt = "Thá»‹ trÆ°á»ng xe Ä‘iá»‡n Viá»‡t Nam Ä‘ang bÃ¹ng ná»• vá»›i hÃ ng loáº¡t máº«u xe má»›i tá»« VinFast, BYD vÃ  cÃ¡c thÆ°Æ¡ng hiá»‡u quá»‘c táº¿...",
                    Content = "Thá»‹ trÆ°á»ng xe Ä‘iá»‡n Viá»‡t Nam Ä‘ang bÃ¹ng ná»• vá»›i hÃ ng loáº¡t máº«u xe má»›i tá»« VinFast, BYD vÃ  cÃ¡c thÆ°Æ¡ng hiá»‡u quá»‘c táº¿. Xu hÆ°á»›ng nÃ y Ä‘ang thay Ä‘á»•i cÃ¡ch ngÆ°á»i Viá»‡t di chuyá»ƒn.",
                    Image = "https://images.unsplash.com/photo-1593941707882-a5bba14938c7?w=800&q=80",
                    Category = "Xu HÆ°á»›ng",
                    ReadTime = "5 phÃºt",
                    PublishedAt = new DateTime(2024, 12, 15)
                },
                new News
                {
                    Slug = "meo-bao-duong-xe",
                    Title = "7 máº¹o báº£o dÆ°á»¡ng xe Ã´ tÃ´ giÃºp kÃ©o dÃ i tuá»•i thá» Ä‘á»™ng cÆ¡",
                    Excerpt = "Báº£o dÆ°á»¡ng Ä‘Ãºng cÃ¡ch khÃ´ng chá»‰ giÃºp xe váº­n hÃ nh á»•n Ä‘á»‹nh mÃ  cÃ²n tiáº¿t kiá»‡m chi phÃ­ sá»­a chá»¯a vá» lÃ¢u dÃ i...",
                    Content = "Báº£o dÆ°á»¡ng Ä‘Ãºng cÃ¡ch khÃ´ng chá»‰ giÃºp xe váº­n hÃ nh á»•n Ä‘á»‹nh mÃ  cÃ²n tiáº¿t kiá»‡m chi phÃ­ sá»­a chá»¯a vá» lÃ¢u dÃ i. DÆ°á»›i Ä‘Ã¢y lÃ  7 máº¹o quan trá»ng.",
                    Image = "https://images.unsplash.com/photo-1632823471406-4c5c7e4c6f24?w=800&q=80",
                    Category = "Báº£o DÆ°á»¡ng",
                    ReadTime = "4 phÃºt",
                    PublishedAt = new DateTime(2024, 12, 10)
                },
                new News
                {
                    Slug = "so-sanh-xe-hang-sang",
                    Title = "So sÃ¡nh BMW X5 vs Audi Q7 vs Mercedes GLE 2024",
                    Excerpt = "Ba Ã´ng lá»›n SUV háº¡ng sang chÃ¢u Ã‚u Ä‘á»‘i Ä‘áº§u trá»±c tiáº¿p trong bÃ i kiá»ƒm tra toÃ n diá»‡n vá» váº­n hÃ nh, tiá»‡n nghi vÃ  giÃ¡ trá»‹...",
                    Content = "Ba Ã´ng lá»›n SUV háº¡ng sang chÃ¢u Ã‚u Ä‘á»‘i Ä‘áº§u trá»±c tiáº¿p trong bÃ i kiá»ƒm tra toÃ n diá»‡n vá» váº­n hÃ nh, tiá»‡n nghi vÃ  giÃ¡ trá»‹.",
                    Image = "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&q=80",
                    Category = "So SÃ¡nh",
                    ReadTime = "8 phÃºt",
                    PublishedAt = new DateTime(2024, 12, 5)
                }
            );
            await db.SaveChangesAsync();
        }
    }

    // Đồng bộ tất cả nhân viên (trừ admin) về email/tên/mật khẩu chuẩn mới
    private static async Task SyncStaffAccountsAsync(AppDbContext db, string staffHash)
    {
        // Bảng ánh xạ: role → (email mới, tên hiển thị)
        var map = new Dictionary<string, (string Email, string Name)>
        {
            [Roles.Sale]       = ("kinhdoanh@gmail.com", "Kinh Doanh"),
            [Roles.Warehouse]  = ("kho@gmail.com",       "Nhân Viên Kho"),
            [Roles.Accounting] = ("ketoan@gmail.com",    "Kế Toán"),
            [Roles.Service]    = ("kythuat@gmail.com",   "Kỹ Thuật"),
            [Roles.Manager]    = ("quanly@gmail.com",    "Quản Lý"),
            [Roles.Staff]      = ("nhanvien@gmail.com",  "Nhân Viên"),
        };

        var allStaff = await db.Users
            .Where(u => u.Role != Roles.Admin && u.Role != Roles.Customer)
            .ToListAsync();

        bool changed = false;
        foreach (var (role, info) in map)
        {
            // Tìm nhân viên theo role (lấy người đầu tiên nếu có nhiều)
            var user = allStaff.FirstOrDefault(u => u.Role == role);
            if (user == null)
            {
                // Tạo mới nếu chưa có
                db.Users.Add(new User
                {
                    Email         = info.Email,
                    Name          = info.Name,
                    Role          = role,
                    Branch        = "TP. Hồ Chí Minh",
                    PasswordHash  = staffHash,
                    EmailVerified = true
                });
                changed = true;
            }
            else
            {
                // Cập nhật email, tên, mật khẩu về chuẩn mới
                bool needUpdate = user.Email != info.Email || user.Name != info.Name;
                if (needUpdate)
                {
                    user.Email        = info.Email;
                    user.Name         = info.Name;
                    user.PasswordHash = staffHash;
                    user.UpdatedAt    = DateTime.UtcNow;
                    changed = true;
                }
                // Luôn đồng bộ mật khẩu về 123456
                else if (!BCrypt.Net.BCrypt.Verify("123456", user.PasswordHash ?? ""))
                {
                    user.PasswordHash = staffHash;
                    user.UpdatedAt    = DateTime.UtcNow;
                    changed = true;
                }
            }
        }

        if (changed) await db.SaveChangesAsync();
    }

    private static async Task SeedTestScenarioAsync(AppDbContext db)
    {
        // Idempotent — chỉ chạy 1 lần
        if (await db.CustomerProfiles.AnyAsync()) return;

        // Lấy nhân viên
        var saleId = (await db.Users.FirstOrDefaultAsync(u => u.Role == Roles.Sale))?.Id;
        var whId   = (await db.Users.FirstOrDefaultAsync(u => u.Role == Roles.Warehouse))?.Id;
        var ktId   = (await db.Users.FirstOrDefaultAsync(u => u.Role == Roles.Accounting))?.Id;
        var svcId  = (await db.Users.FirstOrDefaultAsync(u => u.Role == Roles.Service))?.Id;
        if (saleId == null || whId == null) return;

        // Lấy 20 khách hàng test theo thứ tự 1→20
        var customers = (await db.Users
            .Where(u => u.Role == Roles.Customer && u.Email.StartsWith("khachhang"))
            .ToListAsync())
            .OrderBy(u => int.TryParse(u.Email.Replace("khachhang", "").Replace("@gmail.com", ""), out var n) ? n : 99)
            .ToList();
        if (customers.Count < 20) return;

        // Lấy xe (cần ít nhất 5)
        var cars = await db.Cars.Where(c => c.Status == "approved").ToListAsync();
        if (cars.Count < 3) return;

        // ── Hàm chọn xe theo index tuần hoàn ──
        Car C(int i) => cars[i % cars.Count];

        var sources   = new[] { "walk-in","online","referral","event","other","walk-in","online","referral","event","other","walk-in","online","referral","event","other","walk-in","online","referral","event","other" };
        var interests = new[] { C(0).Name, C(1).Name, C(2).Name, C(3).Name, C(4).Name, C(0).Name, C(1).Name, C(2).Name, C(3).Name, C(4).Name, C(0).Name, C(1).Name, C(2).Name, C(3).Name, C(4).Name, C(0).Name, C(1).Name, C(2).Name, C(3).Name, C(4).Name };

        // ── 1. CustomerProfile + Notes cho tất cả 20 KH ──
        for (int i = 0; i < 20; i++)
        {
            var u = customers[i];
            db.CustomerProfiles.Add(new CustomerProfile
            {
                CustomerId         = u.Id,
                Source             = sources[i],
                InterestedCarModel = interests[i],
                Phone              = $"09{(10000000 + i * 11111111 % 89999999):D8}",
                Summary            = $"Khách test #{i + 1}. Quan tâm {interests[i]}. Ngân sách ~{C(i).Price / 1_000_000:N0}M ₫."
            });
            db.CustomerNotes.Add(new CustomerNote { CustomerId = u.Id, Content = $"Lần đầu tiếp xúc, khách hỏi thông tin {interests[i]} và chính sách bảo hành.", CreatedById = saleId.Value });
            if (i < 15)
                db.CustomerNotes.Add(new CustomerNote { CustomerId = u.Id, Content = "Đã gửi báo giá và brochure, hẹn khách quay lại lái thử.", CreatedById = saleId.Value });
        }
        await db.SaveChangesAsync();

        // ── 2. TestDrive: KH4 pending / KH5 confirmed / KH6 completed ──
        db.TestDrives.AddRange(
            new TestDrive { CustomerId = customers[3].Id, CarId = C(0).Id, LicensePlate = "51G-11001", ScheduledDate = DateTime.UtcNow.AddDays(3), StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(10,0,0), Status = "pending",   Notes = "Khách muốn lái buổi sáng", CreatedById = saleId.Value },
            new TestDrive { CustomerId = customers[4].Id, CarId = C(1).Id, LicensePlate = "51G-11002", ScheduledDate = DateTime.UtcNow.AddDays(1), StartTime = new TimeSpan(14,0,0), EndTime = new TimeSpan(15,0,0), Status = "confirmed", Notes = "Đã xác nhận lịch", CreatedById = saleId.Value },
            new TestDrive { CustomerId = customers[5].Id, CarId = C(2).Id, LicensePlate = "51G-11003", ScheduledDate = DateTime.UtcNow.AddDays(-5), StartTime = new TimeSpan(10,0,0), EndTime = new TimeSpan(11,0,0), Status = "completed", Notes = "Khách hài lòng, sẽ quyết định trong tuần", CreatedById = saleId.Value }
        );
        await db.SaveChangesAsync();

        // ── 3. VehicleUnit + Order + PDI cho KH7→KH17 ──
        var extColors = new[] { "Trắng Ngọc Trai","Đen Huyền","Xám Titan","Đỏ Pha Lê","Xanh Dương","Vàng Cát","Trắng Tinh","Nâu Đồng","Xanh Rêu","Bạc Ánh Kim","Đen Bóng" };
        var intColors = new[] { "Đen","Đen","Be","Đen","Be","Đen","Đen","Be","Đen","Be","Đen" };

        // KH7-17 = customer index 6-16, order index 0-10
        var orderStatuses = new[]
        {
            "Chờ kế toán duyệt tiền",         // KH7  [0]
            "Chờ kế toán duyệt tiền",         // KH8  [1]
            "Chờ kế toán duyệt tiền",         // KH9  [2] - PDI fail → repair
            "Đã đặt cọc — Chờ thanh toán đủ", // KH10 [3]
            "Đã đặt cọc — Chờ thanh toán đủ", // KH11 [4]
            "Đã đặt cọc — Chờ thanh toán đủ", // KH12 [5] + bank loan
            "Chờ bàn giao xe",                 // KH13 [6] - pending delivery
            "Chờ bàn giao xe",                 // KH14 [7] - delivery inprogress
            "Chờ bàn giao xe",                 // KH15 [8] - delivery done
            "Hoàn tất",                        // KH16 [9] - sold
            "Hoàn tất",                        // KH17 [10] - sold
        };
        var vuStatuses = new[] { "reserved","reserved","repair","reserved","reserved","reserved","reserved","reserved","reserved","sold","sold" };
        var pdiPass    = new[] { true,true,false,true,true,true,true,true,true,true,true };

        var orders   = new List<Order>();
        var vehicles = new List<VehicleUnit>();

        for (int i = 0; i < 11; i++)
        {
            var cust  = customers[6 + i];
            var car   = C(i);
            var oDate = DateTime.UtcNow.AddDays(-(25 - i * 2));

            var vu = new VehicleUnit
            {
                CarId         = car.Id,
                Vin           = $"VF3SEED{i + 1:D3}",
                EngineNumber  = $"ENGSEED{i + 1:D3}",
                ExteriorColor = extColors[i],
                InteriorColor = intColors[i],
                PurchasePrice = car.Price * 85 / 100,
                ListPrice     = car.Price,
                Status        = vuStatuses[i],
                CreatedById   = whId
            };
            db.VehicleUnits.Add(vu);
            vehicles.Add(vu);

            var order = new Order
            {
                OrderCode    = $"HD-{oDate:yyyyMMdd}-{1001 + i:D4}",
                CustomerId   = cust.Id,
                CarId        = car.Id,
                VehicleUnitId= vu.Id,
                CarName      = car.Name,
                CustomerName = cust.Name,
                OrderType    = "buy",
                Amount       = car.Price,
                DepositAmount= car.Price / 10,
                Status       = orderStatuses[i],
                StaffId      = saleId,
                Notes        = $"Đơn test {cust.Name}",
                CreatedAt    = oDate,
                UpdatedAt    = oDate.AddDays(1)
            };
            db.Orders.Add(order);
            orders.Add(order);
        }
        await db.SaveChangesAsync();

        // PDI cho tất cả xe
        for (int i = 0; i < 11; i++)
        {
            var pdi = new PdiChecklist
            {
                VehicleUnitId   = vehicles[i].Id,
                ExteriorPassed  = pdiPass[i],
                InteriorPassed  = true,
                ElectricalPassed= true,
                EnginePassed    = true,
                TirePassed      = true,
                Notes           = pdiPass[i] ? "Tất cả hạng mục đạt." : "Ngoại thất có vết xước cần sơn lại.",
                InspectorId     = whId.Value
            };
            if (!pdiPass[i])
                pdi.Defects.Add(new PdiDefect { Category = "Ngoại thất (sơn, kính, đèn, gương)", Reason = "Vết xước dọc cánh cửa trước phải, cần sơn lại trước giao xe." });
            db.PdiChecklists.Add(pdi);
        }
        await db.SaveChangesAsync();

        // ── 4. PaymentReceipts: KH10-17 deposit, KH13-17 full ──
        var accId = ktId ?? saleId.Value;
        for (int i = 3; i < 11; i++) // order index 3→10 = KH10→KH17
        {
            var rDate = orders[i].CreatedAt.AddDays(1);
            db.PaymentReceipts.Add(new PaymentReceipt
            {
                ReceiptCode   = $"PT-{rDate:yyyyMMdd}-{2001 + i:D4}",
                OrderId       = orders[i].Id,
                Amount        = orders[i].DepositAmount,
                PaymentType   = "deposit",
                ConfirmedById = accId,
                ConfirmedAt   = rDate,
                Notes         = "Thanh toán đặt cọc giữ xe"
            });
            if (i >= 6) // KH13-17 đã thanh toán đủ
            {
                db.PaymentReceipts.Add(new PaymentReceipt
                {
                    ReceiptCode   = $"PT-{rDate:yyyyMMdd}-{3001 + i:D4}",
                    OrderId       = orders[i].Id,
                    Amount        = orders[i].Amount - orders[i].DepositAmount,
                    PaymentType   = "full",
                    ConfirmedById = accId,
                    ConfirmedAt   = rDate.AddDays(7),
                    Notes         = "Thanh toán phần còn lại — đủ 100%"
                });
            }
        }
        await db.SaveChangesAsync();

        // ── 5. BankLoan cho KH12 (order index 5) ──
        db.BankLoans.Add(new BankLoan
        {
            OrderId      = orders[5].Id,
            BankName     = "Vietcombank",
            LoanAmount   = orders[5].Amount * 7 / 10,
            InterestRate = 9.5m,
            LoanYears    = 5,
            Status       = "pending",
            Notes        = "Hồ sơ đang chờ ngân hàng phê duyệt"
        });
        await db.SaveChangesAsync();

        // ── 6. PreDeliveryOrders: KH13-17 (order index 6-10) ──
        var pdoStatuses = new[] { "pending","inprogress","done","done","done" };
        for (int i = 6; i < 11; i++)
        {
            var pdo = new PreDeliveryOrder
            {
                OrderId      = orders[i].Id,
                VehicleUnitId= vehicles[i].Id,
                InstructionsJson = "Rửa xe, kiểm tra áp suất lốp, nạp nhiên liệu đầy, lắp phụ kiện đi kèm",
                Status       = pdoStatuses[i - 6],
                AssignedToId = whId,
                CreatedAt    = orders[i].CreatedAt.AddDays(8)
            };
            if (pdoStatuses[i - 6] == "done")
                pdo.CompletedAt = orders[i].CreatedAt.AddDays(10);
            db.PreDeliveryOrders.Add(pdo);
        }
        await db.SaveChangesAsync();

        // ── 7. Commission: KH16-17 (order index 9-10) ──
        for (int i = 9; i < 11; i++)
        {
            db.Commissions.Add(new Commission
            {
                StaffId   = saleId.Value,
                OrderId   = orders[i].Id,
                Rate      = 0.01m,
                Amount    = (long)(orders[i].Amount * 0.01m),
                Month     = DateTime.UtcNow.Month,
                Year      = DateTime.UtcNow.Year,
                IsPaid    = false
            });
        }
        await db.SaveChangesAsync();

        // ── 8. PaymentVouchers (phiếu chi mẫu) ──
        var pcDate = DateTime.UtcNow.AddDays(-10);
        db.PaymentVouchers.AddRange(
            new PaymentVoucher { VoucherCode = $"PC-{pcDate:yyyyMMdd}-5001", Category = "Lương", Amount = 25_000_000, Recipient = "Toàn bộ nhân viên tháng " + DateTime.UtcNow.Month, Description = "Chi lương tháng " + DateTime.UtcNow.Month + "/" + DateTime.UtcNow.Year, CreatedById = ktId ?? saleId.Value, CreatedAt = pcDate },
            new PaymentVoucher { VoucherCode = $"PC-{pcDate:yyyyMMdd}-5002", Category = "Vật tư", Amount = 3_500_000, Recipient = "Cửa hàng Nguyễn Gia", Description = "Mua vật tư vệ sinh, dụng cụ garage", CreatedById = ktId ?? saleId.Value, CreatedAt = pcDate.AddDays(2) },
            new PaymentVoucher { VoucherCode = $"PC-{pcDate:yyyyMMdd}-5003", Category = "Tiếp khách", Amount = 1_800_000, Recipient = "Nhà hàng Phú Gia", Description = "Tiếp khách đối tác VinFast khu vực", CreatedById = ktId ?? saleId.Value, CreatedAt = pcDate.AddDays(4) }
        );
        await db.SaveChangesAsync();

        // ── 9. SpareParts ──
        db.SpareParts.AddRange(
            new SparePart { PartCode = "LO-001", Name = "Nhớt động cơ 5W-30 (4L)", Stock = 48, MinStock = 10, UnitPrice = 320_000, Unit = "chai" },
            new SparePart { PartCode = "LO-002", Name = "Lọc dầu động cơ", Stock = 30, MinStock = 8, UnitPrice = 95_000, Unit = "cái" },
            new SparePart { PartCode = "LO-003", Name = "Lọc gió động cơ", Stock = 20, MinStock = 5, UnitPrice = 180_000, Unit = "cái" },
            new SparePart { PartCode = "PH-001", Name = "Má phanh trước", Stock = 12, MinStock = 4, UnitPrice = 750_000, Unit = "bộ" },
            new SparePart { PartCode = "PH-002", Name = "Đĩa phanh trước", Stock = 3, MinStock = 4, UnitPrice = 1_200_000, Unit = "cái" }, // sắp hết
            new SparePart { PartCode = "BU-001", Name = "Bugi đánh lửa Ngk", Stock = 60, MinStock = 16, UnitPrice = 85_000, Unit = "cái" },
            new SparePart { PartCode = "AC-001", Name = "Lọc điều hoà cabin", Stock = 25, MinStock = 6, UnitPrice = 150_000, Unit = "cái" }
        );
        await db.SaveChangesAsync();

        // ── 10. ServiceTickets: KH18 received / KH19 completed / KH20 paid ──
        var techId = svcId ?? saleId.Value;
        var svcDate = DateTime.UtcNow.AddDays(-8);

        var t18 = new ServiceTicket
        {
            TicketCode = $"DV-{svcDate:yyyyMMdd}-6001", LicensePlate = "51G-20001",
            CustomerName = customers[17].Name, CustomerPhone = "0909111001",
            Odometer = 10200, Status = "received",
            Description = "Bảo dưỡng định kỳ 10.000km: thay nhớt, lọc gió, kiểm tra phanh.",
            CreatedById = techId, CreatedAt = svcDate
        };
        var t19 = new ServiceTicket
        {
            TicketCode = $"DV-{svcDate.AddDays(3):yyyyMMdd}-6002", LicensePlate = "51G-20002",
            CustomerName = customers[18].Name, CustomerPhone = "0909111002",
            Odometer = 45600, Status = "completed",
            Description = "Tiếng ồn hộp số khi tăng tốc, xe rung ở tốc độ cao.",
            AssignedTechnicianId = techId,
            LaborCost = 800_000, TotalPartsCost = 1_200_000, TotalAmount = 2_000_000,
            CreatedById = techId, CreatedAt = svcDate.AddDays(3), CompletedAt = svcDate.AddDays(5)
        };
        var t20 = new ServiceTicket
        {
            TicketCode = $"DV-{svcDate.AddDays(5):yyyyMMdd}-6003", LicensePlate = "51G-20003",
            CustomerName = customers[19].Name, CustomerPhone = "0909111003",
            Odometer = 28900, Status = "paid",
            Description = "Đèn pha không đủ sáng, kiểm tra hệ thống điện và thay bóng đèn.",
            AssignedTechnicianId = techId,
            LaborCost = 500_000, TotalPartsCost = 350_000, TotalAmount = 850_000,
            CreatedById = techId, CreatedAt = svcDate.AddDays(5), CompletedAt = svcDate.AddDays(6)
        };
        db.ServiceTickets.AddRange(t18, t19, t20);
        await db.SaveChangesAsync();
    }

    private static async Task SeedTestCustomersAsync(AppDbContext db)
    {
        var customerHash = BCrypt.Net.BCrypt.HashPassword("123456");
        for (int i = 1; i <= 20; i++)
        {
            var email = $"khachhang{i}@gmail.com";
            if (!await db.Users.AnyAsync(u => u.Email == email))
            {
                db.Users.Add(new User
                {
                    Email         = email,
                    Name          = $"Khách Hàng {i}",
                    Role          = Roles.Customer,
                    Branch        = "TP. Hồ Chí Minh",
                    PasswordHash  = customerHash,
                    EmailVerified = true,
                    IsActive      = true
                });
            }
        }
        await db.SaveChangesAsync();
    }

    private static List<Car> AllCars() =>
    [
        // â"€â"€ Xe Ä‘Ã£ cÃ³ sáºµn â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€
        new Car
        {
            Slug = "mercedes-e300",
            Name = "Mercedes E300 AMG",
            Brand = "Mercedes-Benz",
            Type = "Sedan",
            Fuel = "XÄƒng",
            Price = 3100000000,
            PriceDisplay = "3,100,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&q=80",
            ImagesJson = "[]",
            Badge = "BÃ¡n Cháº¡y",
            SpecsJson = """{"engine":"2.0L Turbo","power":"258 HP","torque":"370 Nm","acceleration":"6.2s (0-100)","topSpeed":"250 km/h","fuelConsumption":"7.5L/100km","transmission":"9G-Tronic","seats":"5"}""",
            FeaturesJson = """["Gháº¿ massage","MÃ n hÃ¬nh 12.3\"","Cá»­a sá»• trá»i Panorama","MBUX AI","Äá»— xe tá»± Ä‘á»™ng"]""",
            RentalPricePerDay = 3500000,
            Status = "approved"
        },
        new Car
        {
            Slug = "bmw-x5",
            Name = "BMW X5 xDrive40i",
            Brand = "BMW",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 4200000000,
            PriceDisplay = "4,200,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Má»›i Nháº¥t",
            SpecsJson = """{"engine":"3.0L TwinPower Turbo","power":"340 HP","torque":"450 Nm","acceleration":"5.5s (0-100)","topSpeed":"250 km/h","fuelConsumption":"9.0L/100km","transmission":"Steptronic 8 cáº¥p","seats":"7"}""",
            FeaturesJson = """["xDrive AWD","MÃ n hÃ¬nh cong 14.9\"","Harman Kardon","iDrive 8","Adaptive LED"]""",
            RentalPricePerDay = 4500000,
            Status = "approved"
        },
        new Car
        {
            Slug = "toyota-camry",
            Name = "Toyota Camry 2.5Q",
            Brand = "Toyota",
            Type = "Sedan",
            Fuel = "Hybrid",
            Price = 1450000000,
            PriceDisplay = "1,450,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Tiáº¿t Kiá»‡m",
            SpecsJson = """{"engine":"2.5L Hybrid","power":"218 HP","torque":"221 Nm","acceleration":"7.0s (0-100)","topSpeed":"210 km/h","fuelConsumption":"4.2L/100km","transmission":"E-CVT","seats":"5"}""",
            FeaturesJson = """["Toyota Safety Sense","JBL 9 loa","Sáº¡c khÃ´ng dÃ¢y","MÃ n hÃ¬nh 9\"","Adaptive Cruise"]""",
            RentalPricePerDay = 1800000,
            Status = "approved"
        },
        new Car
        {
            Slug = "vinfast-vf9",
            Name = "VinFast VF 9 Eco",
            Brand = "VinFast",
            Type = "SUV",
            Fuel = "Äiá»‡n",
            Price = 1890000000,
            PriceDisplay = "1,890,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1593941707882-a5bba14938c7?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Xe Äiá»‡n",
            SpecsJson = """{"engine":"Dual Motor Electric","power":"402 HP","torque":"640 Nm","acceleration":"5.9s (0-100)","topSpeed":"200 km/h","fuelConsumption":"21 kWh/100km","transmission":"Single Speed","seats":"7"}""",
            FeaturesJson = """["Autopilot cÆ¡ báº£n","Sáº¡c nhanh DC 150kW","MÃ n hÃ¬nh 15.6\"","7 chá»— rá»™ng rÃ£i","ÄÃ¨n Matrix LED"]""",
            RentalPricePerDay = 2200000,
            Status = "approved"
        },
        new Car
        {
            Slug = "audi-q7",
            Name = "Audi Q7 55 TFSI",
            Brand = "Audi",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 5800000000,
            PriceDisplay = "5,800,000,000 đ",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&q=80",
            ImagesJson = "[]",
            SpecsJson = """{"engine":"3.0L TFSI V6","power":"340 HP","torque":"500 Nm","acceleration":"5.7s (0-100)","topSpeed":"250 km/h","fuelConsumption":"9.5L/100km","transmission":"Tiptronic 8 cáº¥p","seats":"7"}""",
            FeaturesJson = """["Quattro AWD","Virtual Cockpit 12.3\"","Bang & Olufsen","Air Suspension","Night Vision"]""",
            RentalPricePerDay = 5500000,
            Status = "approved"
        },
        new Car
        {
            Slug = "honda-crv",
            Name = "Honda CR-V e:HEV RS",
            Brand = "Honda",
            Type = "SUV",
            Fuel = "Hybrid",
            Price = 1280000000,
            PriceDisplay = "1,280,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Phá»• Biáº¿n",
            SpecsJson = """{"engine":"2.0L e:HEV","power":"204 HP","torque":"315 Nm","acceleration":"7.8s (0-100)","topSpeed":"190 km/h","fuelConsumption":"5.1L/100km","transmission":"E-CVT","seats":"5"}""",
            FeaturesJson = """["Honda SENSING","AWD","Wireless Apple CarPlay","Bose 12 loa","Panoramic Roof"]""",
            RentalPricePerDay = 1500000,
            Status = "approved"
        },

        // â"€â"€ VinFast â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€
        new Car
        {
            Slug = "vinfast-vf3",
            Name = "VinFast VF 3",
            Brand = "VinFast",
            Type = "Hatchback",
            Fuel = "Äiá»‡n",
            Price = 240000000,
            PriceDisplay = "240,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&q=80",
            ImagesJson = "[]",
            Badge = "GiÃ¡ Tá»‘t",
            SpecsJson = """{"engine":"Single Motor Electric","power":"109 HP","torque":"135 Nm","acceleration":"11.5s (0-100)","topSpeed":"140 km/h","fuelConsumption":"13.3 kWh/100km","transmission":"Single Speed","seats":"4"}""",
            FeaturesJson = """["Sáº¡c AC 6.6kW","MÃ n hÃ¬nh cáº£m á»©ng 8\"","Äiá»u hoÃ  tá»± Ä‘á»™ng","Apple CarPlay","Cáº£m biáº¿n lÃ¹i"]""",
            RentalPricePerDay = 500000,
            Status = "approved"
        },
        new Car
        {
            Slug = "vinfast-vf5",
            Name = "VinFast VF 5 Plus",
            Brand = "VinFast",
            Type = "SUV",
            Fuel = "Äiá»‡n",
            Price = 528000000,
            PriceDisplay = "528,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1623005329892-4bf39f1b0d5d?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Xe Äiá»‡n",
            SpecsJson = """{"engine":"Single Motor Electric","power":"150 HP","torque":"242 Nm","acceleration":"8.8s (0-100)","topSpeed":"155 km/h","fuelConsumption":"16 kWh/100km","transmission":"Single Speed","seats":"5"}""",
            FeaturesJson = """["Sáº¡c nhanh DC 50kW","MÃ n hÃ¬nh 10.4\"","Cáº£nh bÃ¡o va cháº¡m phÃ­a trÆ°á»›c","Há»— trá»£ giá»¯ lÃ n","Camera 360Â°"]""",
            RentalPricePerDay = 800000,
            Status = "approved"
        },
        new Car
        {
            Slug = "vinfast-vf6",
            Name = "VinFast VF 6 Plus",
            Brand = "VinFast",
            Type = "SUV",
            Fuel = "Äiá»‡n",
            Price = 765000000,
            PriceDisplay = "765,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Xe Äiá»‡n",
            SpecsJson = """{"engine":"Single Motor Electric","power":"201 HP","torque":"310 Nm","acceleration":"6.9s (0-100)","topSpeed":"180 km/h","fuelConsumption":"16.9 kWh/100km","transmission":"Single Speed","seats":"5"}""",
            FeaturesJson = """["Sáº¡c nhanh DC 100kW","MÃ n hÃ¬nh 12.9\"","ADAS Level 2","Há»‡ thá»‘ng Ã¢m thanh 6 loa","Phanh tá»± Ä‘á»™ng kháº©n cáº¥p"]""",
            RentalPricePerDay = 1000000,
            Status = "approved"
        },
        new Car
        {
            Slug = "vinfast-vf7",
            Name = "VinFast VF 7 Plus",
            Brand = "VinFast",
            Type = "SUV",
            Fuel = "Äiá»‡n",
            Price = 950000000,
            PriceDisplay = "950,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1647166545674-37aa7d2fcb4d?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Xe Äiá»‡n",
            SpecsJson = """{"engine":"Dual Motor Electric","power":"348 HP","torque":"500 Nm","acceleration":"5.5s (0-100)","topSpeed":"200 km/h","fuelConsumption":"19 kWh/100km","transmission":"Single Speed","seats":"5"}""",
            FeaturesJson = """["Sáº¡c nhanh DC 150kW","MÃ n hÃ¬nh 14.6\"","ADAS Level 2+","Cá»­a sá»• trá»i Panorama","Há»‡ thá»‘ng Ã¢m thanh Harman"]""",
            RentalPricePerDay = 1400000,
            Status = "approved"
        },
        new Car
        {
            Slug = "vinfast-vf8",
            Name = "VinFast VF 8 Eco",
            Brand = "VinFast",
            Type = "SUV",
            Fuel = "Äiá»‡n",
            Price = 1057000000,
            PriceDisplay = "1,057,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1593941707882-a5bba14938c7?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Xe Äiá»‡n",
            SpecsJson = """{"engine":"Dual Motor Electric","power":"300 HP","torque":"460 Nm","acceleration":"6.5s (0-100)","topSpeed":"200 km/h","fuelConsumption":"20 kWh/100km","transmission":"Single Speed","seats":"5"}""",
            FeaturesJson = """["Autopilot cÆ¡ báº£n","Sáº¡c nhanh DC 150kW","MÃ n hÃ¬nh 15.6\"","5 chá»— thá»ƒ thao","ÄÃ¨n Matrix LED"]""",
            RentalPricePerDay = 1600000,
            Status = "approved"
        },
        new Car
        {
            Slug = "vinfast-vf-wild",
            Name = "VinFast VF Wild",
            Brand = "VinFast",
            Type = "Pickup",
            Fuel = "Äiá»‡n",
            Price = 1500000000,
            PriceDisplay = "1,500,000,000 â‚«",
            Year = 2025,
            Image = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Má»›i Nháº¥t",
            SpecsJson = """{"engine":"Tri Motor Electric","power":"402 HP","torque":"640 Nm","acceleration":"5.1s (0-100)","topSpeed":"200 km/h","fuelConsumption":"24 kWh/100km","transmission":"Single Speed","seats":"5"}""",
            FeaturesJson = """["AWD 3 motor","Táº£i trá»ng 800kg","Sáº¡c nhanh DC 200kW","Off-road mode","Há»‡ thá»‘ng kÃ©o 4500kg"]""",
            RentalPricePerDay = 2000000,
            Status = "approved"
        },
        new Car
        {
            Slug = "vinfast-lux-a2",
            Name = "VinFast Lux A2.0",
            Brand = "VinFast",
            Type = "Sedan",
            Fuel = "XÄƒng",
            Price = 950000000,
            PriceDisplay = "950,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&q=80",
            ImagesJson = "[]",
            SpecsJson = """{"engine":"2.0L Turbo","power":"228 HP","torque":"350 Nm","acceleration":"6.5s (0-100)","topSpeed":"230 km/h","fuelConsumption":"8.5L/100km","transmission":"ZF 8 cáº¥p","seats":"5"}""",
            FeaturesJson = """["Gháº¿ da Nappa","MÃ n hÃ¬nh 10.4\"","Há»‡ thá»‘ng Ã¢m thanh Bose","Cá»­a sá»• trá»i Panorama","Äá»— xe tá»± Ä‘á»™ng"]""",
            RentalPricePerDay = 1300000,
            Status = "approved"
        },
        new Car
        {
            Slug = "vinfast-lux-sa2",
            Name = "VinFast Lux SA2.0",
            Brand = "VinFast",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 1100000000,
            PriceDisplay = "1,100,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=800&q=80",
            ImagesJson = "[]",
            SpecsJson = """{"engine":"2.0L Turbo","power":"228 HP","torque":"350 Nm","acceleration":"7.0s (0-100)","topSpeed":"220 km/h","fuelConsumption":"9.0L/100km","transmission":"ZF 8 cáº¥p","seats":"7"}""",
            FeaturesJson = """["AWD","Gháº¿ da Nappa","MÃ n hÃ¬nh 12.3\"","Cá»­a sá»• trá»i Panorama","7 chá»— rá»™ng rÃ£i"]""",
            RentalPricePerDay = 1500000,
            Status = "approved"
        },
        // â"€â"€ Mercedes-Benz â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€
        new Car
        {
            Slug = "mercedes-a200",
            Name = "Mercedes A200",
            Brand = "Mercedes-Benz",
            Type = "Hatchback",
            Fuel = "XÄƒng",
            Price = 1300000000,
            PriceDisplay = "1,300,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1553440569-bcc63803a83d?w=800&q=80",
            ImagesJson = "[]",
            SpecsJson = """{"engine":"1.3L Turbo","power":"163 HP","torque":"250 Nm","acceleration":"8.0s (0-100)","topSpeed":"227 km/h","fuelConsumption":"6.5L/100km","transmission":"7G-DCT","seats":"5"}""",
            FeaturesJson = """["MBUX infotainment","MÃ n hÃ¬nh 7\" x2","Active Brake Assist","Äiá»u hoÃ  tá»± Ä‘á»™ng","ÄÃ¨n LED"]""",
            RentalPricePerDay = 1500000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mercedes-cla200",
            Name = "Mercedes CLA 200",
            Brand = "Mercedes-Benz",
            Type = "Coupe",
            Fuel = "XÄƒng",
            Price = 1550000000,
            PriceDisplay = "1,550,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1542362567-b07e54358753?w=800&q=80",
            ImagesJson = "[]",
            SpecsJson = """{"engine":"1.3L Turbo","power":"163 HP","torque":"250 Nm","acceleration":"8.2s (0-100)","topSpeed":"228 km/h","fuelConsumption":"6.3L/100km","transmission":"7G-DCT","seats":"5"}""",
            FeaturesJson = """["MBUX","MÃ n hÃ¬nh 10.25\" x2","ÄÃ¨n Ambient 64 mÃ u","Active Brake Assist","Thiáº¿t káº¿ Coupe thá»ƒ thao"]""",
            RentalPricePerDay = 1800000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mercedes-c200",
            Name = "Mercedes C200 Exclusive",
            Brand = "Mercedes-Benz",
            Type = "Sedan",
            Fuel = "XÄƒng",
            Price = 1599000000,
            PriceDisplay = "1,599,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&q=80",
            ImagesJson = "[]",
            Badge = "BÃ¡n Cháº¡y",
            SpecsJson = """{"engine":"1.5L EQ Boost Mild Hybrid","power":"204 HP","torque":"300 Nm","acceleration":"7.3s (0-100)","topSpeed":"240 km/h","fuelConsumption":"6.9L/100km","transmission":"9G-Tronic","seats":"5"}""",
            FeaturesJson = """["MBUX 2.0","MÃ n hÃ¬nh 11.9\" dá»c","ÄÃ¨n Ambient 64 mÃ u","ADAS","Gháº¿ Ä‘iá»‡n nhá»› vá»‹ trÃ­"]""",
            RentalPricePerDay = 2000000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mercedes-c300",
            Name = "Mercedes C300 AMG",
            Brand = "Mercedes-Benz",
            Type = "Sedan",
            Fuel = "XÄƒng",
            Price = 1999000000,
            PriceDisplay = "1,999,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&q=80",
            ImagesJson = "[]",
            Badge = "AMG",
            SpecsJson = """{"engine":"2.0L Turbo AMG","power":"258 HP","torque":"400 Nm","acceleration":"6.0s (0-100)","topSpeed":"250 km/h","fuelConsumption":"8.1L/100km","transmission":"9G-Tronic","seats":"5"}""",
            FeaturesJson = """["AMG body kit","Gháº¿ thá»ƒ thao AMG","MBUX 2.0","Phanh AMG","á»ng xáº£ kÃ©p AMG"]""",
            RentalPricePerDay = 2500000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mercedes-gla200",
            Name = "Mercedes GLA 200",
            Brand = "Mercedes-Benz",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 1750000000,
            PriceDisplay = "1,750,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=800&q=80",
            ImagesJson = "[]",
            SpecsJson = """{"engine":"1.3L Turbo","power":"163 HP","torque":"250 Nm","acceleration":"8.7s (0-100)","topSpeed":"222 km/h","fuelConsumption":"7.0L/100km","transmission":"7G-DCT","seats":"5"}""",
            FeaturesJson = """["MBUX","MÃ n hÃ¬nh 10.25\" x2","Raised suspension","Active Brake Assist","Panoramic sunroof"]""",
            RentalPricePerDay = 2100000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mercedes-glb200",
            Name = "Mercedes GLB 200",
            Brand = "Mercedes-Benz",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 1850000000,
            PriceDisplay = "1,850,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&q=80",
            ImagesJson = "[]",
            SpecsJson = """{"engine":"1.3L Turbo","power":"163 HP","torque":"250 Nm","acceleration":"9.0s (0-100)","topSpeed":"220 km/h","fuelConsumption":"7.2L/100km","transmission":"7G-DCT","seats":"7"}""",
            FeaturesJson = """["7 chá»—","MBUX","HÃ ng gháº¿ 3 tÃ¹y chá»n","Active Brake Assist","Panoramic sunroof"]""",
            RentalPricePerDay = 2200000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mercedes-glc200",
            Name = "Mercedes GLC 200 Exclusive",
            Brand = "Mercedes-Benz",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 2099000000,
            PriceDisplay = "2,099,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=800&q=80",
            ImagesJson = "[]",
            Badge = "BÃ¡n Cháº¡y",
            SpecsJson = """{"engine":"1.5L EQ Boost Mild Hybrid","power":"204 HP","torque":"300 Nm","acceleration":"7.5s (0-100)","topSpeed":"235 km/h","fuelConsumption":"7.5L/100km","transmission":"9G-Tronic","seats":"5"}""",
            FeaturesJson = """["MBUX 2.0","MÃ n hÃ¬nh 11.9\" dá»c","4MATIC AWD tÃ¹y chá»n","ÄÃ¨n Ambient","Active Brake Assist"]""",
            RentalPricePerDay = 2800000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mercedes-glc300",
            Name = "Mercedes GLC 300 AMG 4MATIC",
            Brand = "Mercedes-Benz",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 2499000000,
            PriceDisplay = "2,499,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&q=80",
            ImagesJson = "[]",
            Badge = "AMG",
            SpecsJson = """{"engine":"2.0L Turbo AMG","power":"258 HP","torque":"400 Nm","acceleration":"6.3s (0-100)","topSpeed":"250 km/h","fuelConsumption":"9.0L/100km","transmission":"9G-Tronic","seats":"5"}""",
            FeaturesJson = """["4MATIC AWD","AMG styling","Burmester 15 loa","Gháº¿ massage","Head-up Display"]""",
            RentalPricePerDay = 3200000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mercedes-gle300d",
            Name = "Mercedes GLE 300d 4MATIC",
            Brand = "Mercedes-Benz",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 3350000000,
            PriceDisplay = "3,350,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&q=80",
            ImagesJson = "[]",
            SpecsJson = """{"engine":"2.0L Diesel Turbo","power":"272 HP","torque":"600 Nm","acceleration":"6.8s (0-100)","topSpeed":"230 km/h","fuelConsumption":"7.0L/100km","transmission":"9G-Tronic","seats":"7"}""",
            FeaturesJson = """["4MATIC AWD","7 chá»—","Air Suspension","Burmester 15 loa","Gháº¿ massage hÃ ng 1"]""",
            RentalPricePerDay = 4000000,
            Status = "approved"
        },
        // â"€â"€ Lamborghini â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€
        new Car
        {
            Slug = "lamborghini-revuelto",
            Name = "Lamborghini Revuelto",
            Brand = "Lamborghini",
            Type = "Coupe",
            Fuel = "Hybrid",
            Price = 15500000000,
            PriceDisplay = "15,500,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1544636331-e26879cd4d9b?w=800&q=80",
            ImagesJson = "[]",
            Badge = "V12 Hybrid",
            SpecsJson = """{"engine":"6.5L V12 + 3 Motor Äiá»‡n","power":"1001 HP","torque":"720 Nm","acceleration":"2.5s (0-100)","topSpeed":"350 km/h","fuelConsumption":"Hybrid HPEV","transmission":"ISR 8 cáº¥p","seats":"2"}""",
            FeaturesJson = """["V12 Hybrid HPEV 1001 mÃ£ lá»±c","AWD 4 bÃ¡nh chá»§ Ä‘á»™ng","Khung Carbon Monocoque","Há»‡ thá»‘ng mÃ n hÃ¬nh 3 cá»¥m LDVI","Cháº¿ Ä‘á»™ Corsa / Sport / Strada / Recharge"]""",
            RentalPricePerDay = 50000000,
            Status = "approved"
        },
        new Car
        {
            Slug = "lamborghini-huracan-sto",
            Name = "Lamborghini HuracÃ¡n STO",
            Brand = "Lamborghini",
            Type = "Coupe",
            Fuel = "XÄƒng",
            Price = 9700000000,
            PriceDisplay = "9,700,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1526726538690-5cbf956ae2fd?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Super Trofeo Omologata",
            SpecsJson = """{"engine":"5.2L V10 Naturally Aspirated","power":"630 HP","torque":"565 Nm","acceleration":"3.0s (0-100)","topSpeed":"310 km/h","fuelConsumption":"15.7L/100km","transmission":"7 cáº¥p LDF","seats":"2"}""",
            FeaturesJson = """["75% linh kiá»‡n tá»« xe Ä‘ua GT3","ThÃ¢n xe carbon fiber toÃ n bá»™","Phanh carbon-ceramic CCBS","Ba cháº¿ Ä‘á»™ lÃ¡i STO / Trofeo / Pioggia","MÃ¢m forged aluminium 20\""]""",
            RentalPricePerDay = 35000000,
            Status = "approved"
        },
        new Car
        {
            Slug = "lamborghini-urus-performante",
            Name = "Lamborghini Urus Performante",
            Brand = "Lamborghini",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 6300000000,
            PriceDisplay = "6,300,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1563720223185-11003d516935?w=800&q=80",
            ImagesJson = "[]",
            Badge = "SUV SiÃªu Thá»ƒ Thao",
            SpecsJson = """{"engine":"4.0L V8 Twin-Turbo","power":"666 HP","torque":"850 Nm","acceleration":"3.3s (0-100)","topSpeed":"306 km/h","fuelConsumption":"14.0L/100km","transmission":"8 cáº¥p tá»± Ä‘á»™ng","seats":"5"}""",
            FeaturesJson = """["SUV nhanh nháº¥t tháº¿ giá»›i","AWD Torque Vectoring","Nháº¹ hÆ¡n Urus tiÃªu chuáº©n 47kg","MÃ¢m carbon rÃ¨n 23\"","á»ng xáº£ AkrapoviÄ titanium"]""",
            RentalPricePerDay = 25000000,
            Status = "approved"
        },

        // â"€â"€ Ferrari â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€
        new Car
        {
            Slug = "ferrari-812-competizione",
            Name = "Ferrari 812 Competizione",
            Brand = "Ferrari",
            Type = "Coupe",
            Fuel = "XÄƒng",
            Price = 13800000000,
            PriceDisplay = "13,800,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=800&q=80",
            ImagesJson = "[]",
            Badge = "V12 Äá»‰nh Cao",
            SpecsJson = """{"engine":"6.5L V12 Naturally Aspirated","power":"830 HP","torque":"692 Nm","acceleration":"2.85s (0-100)","topSpeed":"340 km/h","fuelConsumption":"16.1L/100km","transmission":"7 cáº¥p F1 DCT","seats":"2"}""",
            FeaturesJson = """["V12 máº¡nh nháº¥t lá»‹ch sá»­ Ferrari Ä‘Æ°á»ng phá»‘","Há»™p sá»‘ F1 sang sá»‘ cá»±c nhanh 80ms","Lá»‘p Michelin Pilot Sport Cup 2R","CÃ¡nh giÃ³ sau chá»§ Ä‘á»™ng phÃ¡t sinh lá»±c nÃ©n","Giá»›i háº¡n 999 chiáº¿c toÃ n cáº§u"]""",
            RentalPricePerDay = 48000000,
            Status = "approved"
        },
        new Car
        {
            Slug = "ferrari-sf90-stradale",
            Name = "Ferrari SF90 Stradale",
            Brand = "Ferrari",
            Type = "Coupe",
            Fuel = "Hybrid",
            Price = 13000000000,
            PriceDisplay = "13,000,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1583121274602-3e2820c69888?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Flagship Hybrid",
            SpecsJson = """{"engine":"4.0L V8 Biturbo + 3 Motor Äiá»‡n","power":"986 HP","torque":"800 Nm","acceleration":"2.5s (0-100)","topSpeed":"340 km/h","fuelConsumption":"Plug-in Hybrid","transmission":"8 cáº¥p DCT","seats":"2"}""",
            FeaturesJson = """["eManettino 4 cháº¿ Ä‘á»™ lÃ¡i (eDrive / Hybrid / Performance / Qualify)","AWD Ä‘iá»‡n thuáº§n á»Ÿ tá»‘c Ä‘á»™ tháº¥p","KÃ­nh cháº¯n giÃ³ Head-Up Display AR","Gháº¿ racing Assetto Fiorano","Phanh carbon-ceramic 398mm"]""",
            RentalPricePerDay = 45000000,
            Status = "approved"
        },
        new Car
        {
            Slug = "ferrari-purosangue",
            Name = "Ferrari Purosangue",
            Brand = "Ferrari",
            Type = "SUV",
            Fuel = "XÄƒng",
            Price = 11300000000,
            PriceDisplay = "11,300,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Ferrari SUV Äáº§u TiÃªn",
            SpecsJson = """{"engine":"6.5L V12 Naturally Aspirated","power":"715 HP","torque":"716 Nm","acceleration":"3.3s (0-100)","topSpeed":"310 km/h","fuelConsumption":"14.5L/100km","transmission":"8 cáº¥p DCT PDK","seats":"4"}""",
            FeaturesJson = """["SUV Ä‘áº§u tiÃªn trong 75 nÄƒm lá»‹ch sá»­ Ferrari","4 cá»­a â€" cá»­a sau má»Ÿ ngÆ°á»£c chiá»u","V12 Ä‘áº·t phÃ­a trÆ°á»›c giá»¯a","Há»‡ thá»‘ng treo chá»§ Ä‘á»™ng ARS 4 bÃ¡nh","DÃ n Ã¢m thanh Burmester 3D 1280W"]""",
            RentalPricePerDay = 40000000,
            Status = "approved"
        },

        // â"€â"€ McLaren â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€â"€
        new Car
        {
            Slug = "mclaren-p1",
            Name = "McLaren P1",
            Brand = "McLaren",
            Type = "Coupe",
            Fuel = "Hybrid",
            Price = 30000000000,
            PriceDisplay = "30,000,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Hypercar Huyá»n Thoáº¡i",
            SpecsJson = """{"engine":"3.8L V8 Twin-Turbo + Motor Äiá»‡n","power":"903 HP","torque":"900 Nm","acceleration":"2.8s (0-100)","topSpeed":"350 km/h","fuelConsumption":"Hybrid IPAS","transmission":"7 cáº¥p SSG","seats":"2"}""",
            FeaturesJson = """["Há»‡ thá»‘ng Hybrid IPAS tá»©c thÃ¬","Cháº¿ Ä‘á»™ Race vá»›i DRS cÃ¡nh giÃ³ 300mm","Khung Carbon MonoCell II siÃªu nháº¹","Há»‡ thá»‘ng KERS thu nÄƒng lÆ°á»£ng phanh","Sáº£n xuáº¥t giá»›i háº¡n chá»‰ 375 chiáº¿c"]""",
            RentalPricePerDay = 80000000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mclaren-senna",
            Name = "McLaren Senna",
            Brand = "McLaren",
            Type = "Coupe",
            Fuel = "XÄƒng",
            Price = 25000000000,
            PriceDisplay = "25,000,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1544636331-e26879cd4d9b?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Vua ÄÆ°á»ng Äua",
            SpecsJson = """{"engine":"4.0L V8 Twin-Turbo M840T","power":"789 HP","torque":"800 Nm","acceleration":"2.8s (0-100)","topSpeed":"340 km/h","fuelConsumption":"14.5L/100km","transmission":"7 cáº¥p SSG","seats":"2"}""",
            FeaturesJson = """["Lá»±c nÃ©n khÃ´ng khÃ­ 800kg á»Ÿ tá»‘c Ä‘á»™ cao","Phanh CCM-R Plus carbon-ceramic","ThÃ¢n xe carbon fiber toÃ n bá»™","CÃ¡nh giÃ³ chá»§ Ä‘á»™ng khá»•ng lá»" phÃ­a sau","Giá»›i háº¡n 500 chiáº¿c tÃ´n vinh Ayrton Senna"]""",
            RentalPricePerDay = 70000000,
            Status = "approved"
        },
        new Car
        {
            Slug = "mclaren-750s-spider",
            Name = "McLaren 750S Spider",
            Brand = "McLaren",
            Type = "Coupe",
            Fuel = "XÄƒng",
            Price = 8500000000,
            PriceDisplay = "8,500,000,000 â‚«",
            Year = 2024,
            Image = "https://images.unsplash.com/photo-1580274455191-1c62238fa1f1?w=800&q=80",
            ImagesJson = "[]",
            Badge = "Spider Mui Tráº§n",
            SpecsJson = """{"engine":"4.0L V8 Twin-Turbo M840T","power":"750 HP","torque":"800 Nm","acceleration":"2.8s (0-100)","topSpeed":"332 km/h","fuelConsumption":"12.5L/100km","transmission":"7 cáº¥p SSG","seats":"2"}""",
            FeaturesJson = """["Mui cá»©ng Ä‘iá»‡n xáº¿p gá»n chá»‰ 11 giÃ¢y","Há»‡ thá»‘ng treo Proactive Chassis Control II","Khung Carbon MonoCell II-T","Variable Drift Control há»— trá»£ drift","Brake Steer tá»‘i Æ°u hÃ³a cua gÃ³c háº¹p"]""",
            RentalPricePerDay = 30000000,
            Status = "approved"
        },
    ];
}

