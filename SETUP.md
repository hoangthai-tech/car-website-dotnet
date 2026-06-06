# AutoElite — ASP.NET Core + SQL Server

## Yêu cầu
- .NET 10 SDK
- SQL Server (LocalDB hoặc SQL Server Express)

## Chạy project

### 1. Cập nhật connection string (nếu dùng SQL Server thực)
Mở `appsettings.json`, sửa `DefaultConnection`:
```
"Server=YOUR_SERVER;Database=AutoEliteDb;User Id=YOUR_USER;Password=YOUR_PASS;"
```
LocalDB (mặc định, không cần cài thêm):
```
"Server=(localdb)\\mssqllocaldb;Database=AutoEliteDb;Trusted_Connection=True;"
```

### 2. Chạy app (tự động tạo DB + seed data)
```bash
cd car-website-dotnet
dotnet run
```
Mở trình duyệt: https://localhost:7xxx (xem port trong terminal)

## Tài khoản demo
| Role | Email | Mật khẩu |
|------|-------|-----------|
| Admin | admin@autoelite.vn | Admin@123 |
| Manager | manager@autoelite.vn | Manager@123 |
| Staff | staff@autoelite.vn | Staff@123 |

## Cấu trúc project
```
Controllers/        — HomeController, XeController, TinTucController...
Data/               — AppDbContext, DbSeeder
Models/             — Car, User, Order, AuditLog, DiscountRequest, News
Migrations/         — EF Core migrations
Views/              — Razor Views cho từng trang
  Shared/           — _Layout.cshtml, _DashboardLayout.cshtml
  Home/             — Trang chủ
  Xe/               — Danh sách xe, Chi tiết xe
  TinTuc/           — Tin tức
  ThueXe/           — Thuê xe
  SoSanh/           — So sánh xe
  LienHe/           — Liên hệ
  Account/          — Login, Register, ForgotPassword
  Dashboard/        — Quản trị (admin/manager/staff)
  Profile/          — Hồ sơ khách hàng
SqlServer-Schema.sql — Script SQL tạo bảng
```

## Migration thủ công (nếu cần)
```bash
dotnet-ef database update
```
