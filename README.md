# AutoHT - Website Đại Lý Ô Tô

Website quản lý đại lý ô tô xây dựng bằng ASP.NET Core MVC (.NET 10) + SQL Server.

## Yêu cầu cài đặt

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express hoặc Developer đều được)

## Cách chạy dự án

### 1. Clone repo

```bash
git clone https://github.com/hoangthai-tech/car-website-dotnet.git
cd car-website-dotnet
```

### 2. Tạo file cấu hình

Sao chép file mẫu và điền thông tin thật:

```bash
copy appsettings.Example.json appsettings.json
```

Mở `appsettings.json` và cập nhật:

| Mục | Mô tả |
|-----|-------|
| `ConnectionStrings.DefaultConnection` | Chuỗi kết nối SQL Server |
| `Email.FromEmail` | Gmail dùng để gửi email |
| `Email.AppPassword` | [App Password của Gmail](https://myaccount.google.com/apppasswords) |
| `Authentication.Google` | ClientId & ClientSecret từ Google Cloud Console |
| `Authentication.Facebook` | AppId & AppSecret từ Facebook Developer |

### 3. Chạy ứng dụng

```bash
dotnet run
```

Database sẽ tự động được tạo và migrate khi khởi động lần đầu.

Truy cập tại: **http://localhost:5074**

### Tài khoản mặc định

| Role | Email | Mật khẩu |
|------|-------|----------|
| Admin | admin@autoht.vn | Admin@123 |
| Kinh doanh | kinhdoanh@gmail.com | 123456 |
| Kho | kho@gmail.com | 123456 |
| Kế toán | ketoan@gmail.com | 123456 |
| Kỹ thuật | kythuat@gmail.com | 123456 |
