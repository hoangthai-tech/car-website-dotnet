# TÀI LIỆU TỔNG QUAN DỰ ÁN: AutoHT Car Dealership Website

> **Mục đích file:** Cung cấp thông tin đầy đủ, chi tiết về toàn bộ dự án để hỗ trợ viết báo cáo đồ án cơ sở.
> **Ngày tạo:** 2026-05-21
> **Tên hệ thống:** AutoHT — Hệ thống quản lý và mua bán xe ô tô trực tuyến

---

## MỤC LỤC
1. [Tổng quan dự án](#1-tổng-quan-dự-án)
2. [Công nghệ sử dụng](#2-công-nghệ-sử-dụng)
3. [Kiến trúc hệ thống](#3-kiến-trúc-hệ-thống)
4. [Cấu trúc thư mục](#4-cấu-trúc-thư-mục)
5. [Cơ sở dữ liệu](#5-cơ-sở-dữ-liệu)
6. [Các chức năng chính](#6-các-chức-năng-chính)
7. [Controllers và Actions](#7-controllers-và-actions)
8. [Models (Thực thể dữ liệu)](#8-models-thực-thể-dữ-liệu)
9. [Giao diện người dùng (Views)](#9-giao-diện-người-dùng-views)
10. [Hệ thống xác thực và phân quyền](#10-hệ-thống-xác-thực-và-phân-quyền)
11. [Dữ liệu mẫu (Seed Data)](#11-dữ-liệu-mẫu-seed-data)
12. [Bảo mật](#12-bảo-mật)
13. [Tính năng đặc biệt](#13-tính-năng-đặc-biệt)
14. [Phân tích chi tiết từng module](#14-phân-tích-chi-tiết-từng-module)

---

## 1. TỔNG QUAN DỰ ÁN

### 1.1 Giới thiệu
**AutoHT** là một hệ thống quản lý đại lý xe ô tô toàn diện được xây dựng trên nền tảng ASP.NET Core MVC. Dự án bao gồm hai phần chính:
- **Website khách hàng (Public-facing):** Cho phép người dùng xem xe, đặt mua/thuê, đọc tin tức, so sánh xe.
- **Dashboard quản lý nội bộ (Staff Dashboard):** Hệ thống quản lý dành cho nhân viên với nhiều phân hệ: Bán hàng, CRM, Kho, Kế toán, Dịch vụ.

### 1.2 Mục tiêu dự án
- Xây dựng nền tảng thương mại điện tử cho đại lý ô tô
- Số hóa quy trình bán hàng, quản lý kho và chăm sóc khách hàng
- Tích hợp hệ thống quản lý nội bộ đa vai trò
- Hỗ trợ cả mua xe và thuê xe

### 1.3 Quy mô dự án
| Thành phần | Số lượng |
|---|---|
| Models (Thực thể) | 27 |
| Controllers | 10 |
| Actions (API endpoints) | 50+ |
| Views (Razor pages) | 35+ |
| Database migrations | 9 |
| Packages NuGet | 5 |

---

## 2. CÔNG NGHỆ SỬ DỤNG

### 2.1 Backend
| Công nghệ | Phiên bản | Mục đích |
|---|---|---|
| .NET | 10.0 | Framework chính |
| ASP.NET Core MVC | 10.0 | Web framework |
| Entity Framework Core | 9.0.4 | ORM - Truy cập CSDL |
| EF Core SqlServer | 9.0.4 | SQL Server provider |
| BCrypt.Net-Next | 4.0.3 | Mã hóa mật khẩu |
| MailKit | 4.16.0 | Gửi email SMTP |
| DnsClient | 1.8.0 | Kiểm tra MX record email |
| ASP.NET Core Identity | 9.0.4 | Identity integration |

### 2.2 Frontend
| Công nghệ | Mục đích |
|---|---|
| Razor Views (.cshtml) | Template engine |
| Tailwind CSS (CDN) | CSS framework chính |
| Bootstrap | Grid và components |
| jQuery 3.6+ | DOM manipulation, AJAX |
| jQuery Validation | Xác thực form phía client |
| Alpine.js | Reactive UI (minimal) |

### 2.3 Database
- **DBMS:** Microsoft SQL Server (localhost)
- **Database name:** AutoEliteDb
- **ORM:** Entity Framework Core (Code First)

### 2.4 Dịch vụ ngoài
- **Email:** Gmail SMTP (qua MailKit)
- **3D Models:** Định dạng GLB/GLTF (Three.js model viewer)

---

## 3. KIẾN TRÚC HỆ THỐNG

### 3.1 Mô hình kiến trúc
Dự án sử dụng kiến trúc **MVC (Model-View-Controller)** chuẩn của ASP.NET Core:

```
Client (Browser)
    ↕ HTTP Request/Response
[Razor Views] ← [Controllers] → [DbContext (EF Core)] → [SQL Server]
                      ↕
                [Services]
               (EmailService)
```

### 3.2 Các Pattern sử dụng
1. **MVC Pattern** — Tách biệt Model, View, Controller
2. **Repository Pattern (implicit)** — DbContext đóng vai trò Unit of Work
3. **Dependency Injection** — Constructor injection cho DbContext, EmailService, Logger
4. **Session + Claims-based Identity** — Dual authentication
5. **Slug-based routing** — SEO-friendly URLs

### 3.3 Luồng xử lý Request
```
HTTP Request
    → Routing (Program.cs routes)
    → Middleware (Auth, Session, CORS)
    → Controller Action
    → Business Logic (+ EF Core queries)
    → View Rendering (Razor)
    → HTTP Response
```

---

## 4. CẤU TRÚC THƯ MỤC

```
car-website-dotnet/
├── Controllers/                    # 10 controllers
│   ├── HomeController.cs           # Trang chủ
│   ├── AccountController.cs        # Đăng nhập/đăng ký
│   ├── XeController.cs             # Danh mục và đặt mua xe
│   ├── ProfileController.cs        # Hồ sơ người dùng
│   ├── DashboardController.cs      # Dashboard nội bộ (~35 actions)
│   ├── TinTucController.cs         # Tin tức/blog
│   ├── LienHeController.cs         # Liên hệ
│   ├── SoSanhController.cs         # So sánh xe
│   ├── ThueXeController.cs         # Thuê xe
│   ├── UuDaiController.cs          # Ưu đãi
│   └── TermsPolicyController.cs    # Quản lý điều khoản
│
├── Models/                         # 27 entity models
│   ├── User.cs
│   ├── Roles.cs
│   ├── Car.cs
│   ├── Order.cs
│   ├── VehicleUnit.cs
│   ├── DiscountRequest.cs
│   ├── PaymentReceipt.cs
│   ├── PaymentVoucher.cs
│   ├── Commission.cs
│   ├── BankLoan.cs
│   ├── PdiChecklist.cs
│   ├── PdiDefect.cs
│   ├── PreDeliveryOrder.cs
│   ├── CustomerProfile.cs
│   ├── CustomerNote.cs
│   ├── TestDrive.cs
│   ├── ServiceTicket.cs
│   ├── SparePart.cs
│   ├── SparePartUsage.cs
│   ├── News.cs
│   ├── TermsOfService.cs
│   ├── UserTermAgreement.cs
│   ├── AuditLog.cs
│   ├── TodoItem.cs
│   └── ErrorViewModel.cs
│
├── Views/                          # Razor views
│   ├── Home/                       # Trang chủ, Privacy, Terms
│   ├── Account/                    # Login, Register, VerifyCode, ForgotPassword
│   ├── Xe/                         # Danh sách, Chi tiết, 3D Viewer
│   ├── Profile/                    # Hồ sơ, Đơn hàng
│   ├── Dashboard/                  # 30+ views quản lý nội bộ
│   ├── TinTuc/                     # Danh sách, Chi tiết tin
│   ├── ThueXe/                     # Thuê xe
│   ├── UuDai/                      # Ưu đãi
│   ├── SoSanh/                     # So sánh xe
│   ├── LienHe/                     # Liên hệ
│   ├── TermsPolicy/                # Điều khoản (admin)
│   └── Shared/                     # _Layout, _DashboardLayout, Error
│
├── Data/
│   └── AppDbContext.cs             # EF Core DbContext + DbSets
│
├── Migrations/                     # 9 database migrations
│
├── Services/
│   └── EmailService.cs             # SMTP email via MailKit
│
├── wwwroot/                        # Static files
│   ├── css/site.css                # Custom CSS
│   ├── js/site.js                  # Custom JS
│   ├── images/cars/                # Ảnh xe upload
│   ├── models3d/                   # 40+ file .glb (3D models)
│   └── lib/                        # jQuery, Bootstrap, Validation
│
├── Properties/
│   └── launchSettings.json
│
├── appsettings.json                # Connection string, SMTP config
├── appsettings.Development.json
├── Program.cs                      # App configuration & startup
└── car-website-dotnet.csproj       # Project file & dependencies
```

---

## 5. CƠ SỞ DỮ LIỆU

### 5.1 Tổng quan
- **Tên DB:** AutoEliteDb
- **Engine:** SQL Server
- **Approach:** Code First với EF Core Migrations
- **Số bảng:** 23 DbSets

### 5.2 Lịch sử Migrations
| Migration | Ngày | Nội dung |
|---|---|---|
| InitialCreate | 22/04/2026 | Schema ban đầu (User, Car, Order, News...) |
| AddEmailVerificationAndFKs | 23/04/2026 | Xác thực email, foreign keys |
| AddCarStock | 23/04/2026 | Thêm trường tồn kho cho xe |
| Add3DModelUrl | 23/04/2026 | Hỗ trợ 3D model URL |
| ClearSampleImages | 24/04/2026 | Dọn dữ liệu ảnh mẫu |
| AddRentalFields | 12/05/2026 | Thuê xe: ngày thuê, giá/ngày |
| AddShowroomEntities | 19/05/2026 | VehicleUnit, PDI, Pre-delivery |
| FixDecimalPrecision | 19/05/2026 | Chỉnh độ chính xác số thập phân |
| AddTermsOfService | 20/05/2026 | Điều khoản & tuân thủ pháp lý |

### 5.3 Sơ đồ quan hệ (ERD tóm tắt)

**Nhóm User & Auth:**
```
User (id, email, name, role, passwordHash, avatar, branch, isEmailVerified, isActive)
```

**Nhóm Sản phẩm:**
```
Car (id, slug, name, brand, type, fuel, price, rentalPricePerDay, year,
     imagesJson, specsJson, featuresJson, model3dUrl, stock, status)
```

**Nhóm Đơn hàng:**
```
Order → Customer(User), Car, VehicleUnit, Staff(User)
      → PaymentReceipt (nhiều thanh toán)
      → BankLoan (vay ngân hàng)
      → DiscountRequest (xin giảm giá)
      → Commission (hoa hồng nhân viên)
      → PreDeliveryOrder (chuẩn bị giao xe)
```

**Nhóm Kho:**
```
VehicleUnit → Car, User/CreatedBy
            → PdiChecklist → PdiDefect (khuyết điểm)
```

**Nhóm CRM:**
```
CustomerProfile → User (1:1)
CustomerNote → User/Customer, User/CreatedBy
TestDrive → User/Customer, Car, User/CreatedBy
```

**Nhóm Dịch vụ:**
```
ServiceTicket → User/Technician, User/CreatedBy
             → SparePartUsage → SparePart
```

**Nhóm Compliance:**
```
TermsOfService → User/PublishedBy
UserTermAgreement → User, TermsOfService (composite unique key)
```

### 5.4 Các Foreign Key và Cascade Rules
| Quan hệ | Cascade Rule |
|---|---|
| Order → Customer (User) | Cascade Delete |
| Order → Staff (User) | No Action |
| VehicleUnit → Car | Restrict Delete |
| PdiChecklist → VehicleUnit | Cascade Delete |
| PdiDefect → PdiChecklist | Cascade Delete |
| CustomerProfile → User | Cascade Delete (unique) |
| CustomerNote → User/Customer | Cascade |
| CustomerNote → User/CreatedBy | No Action |
| ServiceTicket → Technician | Set Null |
| SparePartUsage → ServiceTicket | Cascade |
| SparePartUsage → SparePart | Restrict |
| UserTermAgreement → User | Cascade |
| UserTermAgreement → TermsOfService | Cascade |

---

## 6. CÁC CHỨC NĂNG CHÍNH

### 6.1 Website Khách hàng (Public)
| Chức năng | Mô tả |
|---|---|
| Trang chủ | Hero slideshow Ken Burns, xe nổi bật, thống kê, tin tức mới nhất |
| Danh mục xe | Lọc theo hãng, loại, nhiên liệu, giá, tìm kiếm full-text |
| Gợi ý tìm kiếm | AJAX real-time, trả về 6 kết quả nhanh |
| Chi tiết xe | Thông số, tính năng, xe liên quan, CTA mua/thuê |
| Xem xe 3D | GLB model viewer tích hợp (Three.js) |
| Đặt mua xe | Form đặt hàng (mua hoặc thuê) |
| Thuê xe | Danh sách xe thuê với giá/ngày |
| Ưu đãi | Hiển thị xe đang khuyến mãi |
| So sánh xe | So sánh 2 xe cạnh nhau theo thông số |
| Tin tức | Blog, bài viết, slug-based URL |
| Liên hệ | Form liên hệ lưu vào AuditLog |
| Điều khoản | Hiển thị điều khoản dịch vụ hiện hành |

### 6.2 Authentication & User Management
| Chức năng | Mô tả |
|---|---|
| Đăng ký | Tạo tài khoản, gửi OTP xác thực email |
| Xác thực email | OTP 6 chữ số (15 phút), hoặc link token (24h) |
| Đăng nhập | Cookie auth + Session, redirect theo role |
| Đổi mật khẩu | BCrypt verify + hash mật khẩu mới |
| Quên mật khẩu | Giao diện cơ bản (stub) |
| Chấp nhận điều khoản | Popup buộc chấp nhận trước khi vào hệ thống |
| Hồ sơ người dùng | Xem/sửa thông tin cá nhân |
| Lịch sử đơn hàng | Xem đơn hàng đã đặt |

### 6.3 Dashboard Nội bộ — Module Bán hàng (Sales)
| Chức năng | Mô tả |
|---|---|
| KPI tổng quan | Doanh thu, giao xe, tồn kho, khách mới |
| Biểu đồ doanh thu | Line chart 12 tháng |
| Biểu đồ phân loại xe | Pie chart theo loại xe |
| Hiệu suất nhân viên | Số đơn, doanh thu từng sales |
| Quản lý đơn hàng | Danh sách, chi tiết, cập nhật trạng thái (12 trạng thái) |
| Tạo đơn hàng | Form tạo đơn mới |
| Yêu cầu giảm giá | Gửi/duyệt/từ chối DiscountRequest |

### 6.4 Dashboard Nội bộ — Module CRM
| Chức năng | Mô tả |
|---|---|
| Hồ sơ khách hàng | Nguồn khách (walk-in/online/giới thiệu), xe quan tâm |
| Ghi chú khách hàng | Timeline lịch sử liên lạc |
| Lái thử xe | Đặt lịch, xác nhận, hoàn tất, theo dõi trạng thái |
| Nhân viên | Danh sách, phân vai, quản lý |

### 6.5 Dashboard Nội bộ — Module Kho (Warehouse)
| Chức năng | Mô tả |
|---|---|
| Danh mục xe | CRUD xe, upload ảnh (30MB limit), duyệt đăng |
| Xe đơn vị (VehicleUnit) | Theo dõi xe cụ thể theo VIN, số máy, màu sắc |
| PDI Checklist | Kiểm tra trước giao xe (ngoại thất/nội thất/điện/động cơ/lốp) |
| Lỗi PDI | Ghi nhận khuyết điểm kèm ảnh, phân loại |
| Chuẩn bị giao xe | Phiếu hướng dẫn chuẩn bị, phân công nhân viên |
| Tồn kho | Đếm số xe theo từng model |

### 6.6 Dashboard Nội bộ — Module Kế toán (Accounting)
| Chức năng | Mô tả |
|---|---|
| Phiếu thu | Theo dõi thu tiền (đặt cọc, trả góp, toàn bộ, dịch vụ) |
| Phiếu chi | Ghi nhận chi phí theo danh mục |
| Hoa hồng | Tính và theo dõi hoa hồng nhân viên bán hàng |
| Vay ngân hàng | Quản lý hồ sơ vay (lãi suất, kỳ hạn, trạng thái) |

### 6.7 Dashboard Nội bộ — Module Dịch vụ (Service)
| Chức năng | Mô tả |
|---|---|
| Phiếu dịch vụ | Tiếp nhận, sửa chữa, bàn giao xe |
| Phân công kỹ thuật viên | Gán phiếu cho technician cụ thể |
| Chi phí dịch vụ | Công lao động + phụ tùng |
| Kho phụ tùng | Tồn kho phụ tùng, cảnh báo min stock |
| Sử dụng phụ tùng | Theo dõi phụ tùng dùng trong từng phiếu |

### 6.8 Module Admin
| Chức năng | Mô tả |
|---|---|
| Quản lý điều khoản | Tạo/kích hoạt phiên bản điều khoản |
| Thống kê chấp thuận | Xem user nào đã/chưa đồng ý điều khoản |
| Audit Log | Nhật ký hoạt động hệ thống |
| Todo/Task | Quản lý công việc nội bộ |

---

## 7. CONTROLLERS VÀ ACTIONS

### 7.1 HomeController
**Route:** `/`
| Action | Method | Mô tả |
|---|---|---|
| `Index()` | GET | Trang chủ: lấy xe nổi bật, thống kê, tin tức |
| `Terms()` | GET | Hiển thị điều khoản dịch vụ đang active |
| `Error()` | GET | Trang lỗi |

### 7.2 AccountController
**Route:** `/Account/`
| Action | Method | Mô tả |
|---|---|---|
| `Login()` | GET/POST | Đăng nhập, tạo Claims cookie |
| `Register()` | GET/POST | Đăng ký + gửi OTP email |
| `VerifyCode()` | GET/POST | Xác thực OTP 6 chữ số |
| `ResendCode()` | POST | Gửi lại OTP |
| `VerifyEmail()` | GET | Xác thực qua link email |
| `ForgotPassword()` | GET/POST | Quên mật khẩu (stub) |
| `AcceptNewTerms()` | GET/POST | Chấp nhận điều khoản mới |
| `Logout()` | GET | Xóa session, đăng xuất |

### 7.3 XeController
**Route:** `/Xe/` và `/xe/{slug}`
| Action | Method | Mô tả |
|---|---|---|
| `Index()` | GET | Danh sách xe, lọc theo brand/type/fuel/search/price |
| `SearchSuggest()` | GET | AJAX gợi ý tìm kiếm (6 kết quả) |
| `Detail(slug)` | GET | Chi tiết xe, xe liên quan |
| `Viewer3D(slug)` | GET | Trình xem 3D model GLB |
| `DatMua()` | POST | Đặt mua hoặc thuê xe |

### 7.4 ProfileController
**Route:** `/Profile/` `[Authorize]`
| Action | Method | Mô tả |
|---|---|---|
| `Index()` | GET | Xem hồ sơ |
| `Update()` | POST | Cập nhật tên người dùng |
| `ChangePassword()` | POST | Đổi mật khẩu (BCrypt verify) |
| `Orders()` | GET | Danh sách đơn hàng của user |

### 7.5 DashboardController
**Route:** `/Dashboard/` `[Authorize]`
| Action | Method | Module |
|---|---|---|
| `Index()` | GET | KPIs, charts, recent orders |
| `Xe()` | GET | Danh sách xe (admin only) |
| `XeAdd()` | GET/POST | Thêm xe + upload ảnh |
| `XeEdit()` | GET/POST | Sửa xe |
| `DonHang()` | GET | Quản lý đơn hàng |
| `ChiTietDonHang()` | GET | Chi tiết đơn |
| `TaoDonHang()` | GET/POST | Tạo đơn mới |
| `CapNhatTrangThaiDon()` | POST | Cập nhật trạng thái đơn |
| `KhachHang()` | GET | Danh sách khách hàng |
| `ChiTietKhachHang()` | GET | Hồ sơ khách hàng |
| `ThemGhiChu()` | POST | Thêm ghi chú CRM |
| `LaiThu()` | GET/POST | Quản lý lái thử |
| `NhanVien()` | GET | Danh sách nhân viên |
| `ThemNhanVien()` | POST | Thêm nhân viên mới |
| `KhoDanhMucXe()` | GET | Danh mục xe kho |
| `KhoXeUnit()` | GET | Xe đơn vị (VehicleUnit) |
| `ThemXeUnit()` | POST | Thêm xe đơn vị |
| `PdiChecklist()` | GET/POST | PDI kiểm tra xe |
| `ChuanBiXe()` | GET/POST | Chuẩn bị giao xe |
| `KeToanThuChi()` | GET | Thu chi tổng quan |
| `ThemPhieuThu()` | POST | Thêm phiếu thu |
| `KeToanPhieuChi()` | GET/POST | Phiếu chi |
| `KeToanHoaHong()` | GET | Hoa hồng nhân viên |
| `VayNganHang()` | GET/POST | Vay ngân hàng |
| `DichVuPhieuDichVu()` | GET/POST | Phiếu dịch vụ |
| `DichVuKhoPhuTung()` | GET/POST | Kho phụ tùng |
| `AuditLog()` | GET | Nhật ký hoạt động |
| `TodoList()` | GET/POST | Quản lý công việc |
| `TermsPolicy` → (TermsPolicyController) | - | Điều khoản (tách controller) |

### 7.6 TinTucController
**Route:** `/TinTuc/` và `/tin-tuc/{slug}`
| Action | Method | Mô tả |
|---|---|---|
| `Index()` | GET | Danh sách tin tức |
| `Detail(slug)` | GET | Chi tiết bài viết |

### 7.7 LienHeController
**Route:** `/LienHe/`
| Action | Method | Mô tả |
|---|---|---|
| `Index()` | GET | Trang liên hệ |
| `Send()` | POST | Gửi liên hệ → lưu AuditLog |

### 7.8 SoSanhController
**Route:** `/SoSanh/`
| Action | Method | Mô tả |
|---|---|---|
| `Index(slug1, slug2)` | GET | So sánh 2 xe cạnh nhau |

### 7.9 ThueXeController
**Route:** `/ThueXe/`
| Action | Method | Mô tả |
|---|---|---|
| `Index()` | GET | Danh sách xe thuê với giá/ngày |

### 7.10 UuDaiController
**Route:** `/UuDai/`
| Action | Method | Mô tả |
|---|---|---|
| `Index()` | GET | Xe đang khuyến mãi/ưu đãi |

### 7.11 TermsPolicyController
**Route:** `/TermsPolicy/` `[Authorize(Roles="admin")]`
| Action | Method | Mô tả |
|---|---|---|
| `Index()` | GET | Danh sách tất cả phiên bản điều khoản |
| `Publish()` | POST | Tạo phiên bản mới (vô hiệu hóa cũ) |
| `Activate(id)` | POST | Kích hoạt lại phiên bản cũ |
| `Preview(id)` | GET | Xem trước nội dung điều khoản |
| `AgreementStats(id)` | GET | Thống kê user đã chấp nhận |

---

## 8. MODELS (THỰC THỂ DỮ LIỆU)

### 8.1 User.cs
```
- Id (int)
- Email (string, unique)
- Name (string)
- Role (string) — Roles constant
- PasswordHash (string)
- Avatar (string?) — URL ảnh đại diện
- Branch (string?) — Chi nhánh
- EmailVerificationToken (string?)
- EmailVerificationCode (string?) — OTP 6 số
- EmailVerificationExpiry (DateTime?)
- IsEmailVerified (bool)
- IsActive (bool) — Soft delete
- CreatedAt (DateTime)
```

### 8.2 Roles.cs (Constants + Metadata)
```
Constants:
  Admin = "admin"
  Sale = "sale"
  Warehouse = "warehouse"
  Accounting = "accounting"
  Service = "service"
  Manager = "manager"   (backward compat)
  Staff = "staff"       (backward compat)
  Customer = "customer"

Display Names (tiếng Việt):
  admin → "Quản trị viên"
  sale → "Nhân viên bán hàng"
  warehouse → "Nhân viên kho"
  accounting → "Kế toán"
  service → "Kỹ thuật viên"

Badge Colors (CSS):
  admin → "bg-red-100 text-red-800"
  sale → "bg-blue-100 text-blue-800"
  ...
```

### 8.3 Car.cs
```
- Id (int)
- Slug (string, unique) — URL-friendly
- Name (string)
- Brand (string) — Hãng xe
- Type (string) — Loại: Sedan, SUV, Coupe...
- FuelType (string) — Xăng/Diesel/Điện
- Price (decimal) — Giá bán (VNĐ)
- RentalPricePerDay (decimal?) — Giá thuê/ngày
- Year (int) — Năm sản xuất
- ImagesJson (string) — JSON array URLs ảnh
- SpecsJson (string) — JSON thông số kỹ thuật
- FeaturesJson (string) — JSON tính năng
- Model3dUrl (string?) — Đường dẫn file .glb
- Stock (int) — Số lượng tồn kho
- Status (string) — active/draft/discontinued
- IsRentable (bool) — Có thể thuê không
- CreatedAt (DateTime)
```

### 8.4 Order.cs
```
- Id (int)
- OrderCode (string, unique) — Mã đơn tự sinh
- CustomerId (int?) → User
- CarId (int?) → Car
- VehicleUnitId (int?) → VehicleUnit
- StaffId (int?) → User
- Type (string) — "buy" | "rent"
- Status (string) — 12 trạng thái (xem mục 14)
- TotalAmount (decimal)
- DepositAmount (decimal)
- RentalStartDate (DateTime?)
- RentalEndDate (DateTime?)
- Notes (string?)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
```

**12 trạng thái đơn hàng:**
1. `pending` — Chờ xử lý
2. `confirmed` — Đã xác nhận
3. `deposit_paid` — Đã đặt cọc
4. `vehicle_assigned` — Đã phân xe
5. `pdi_pending` — Chờ kiểm tra PDI
6. `pdi_completed` — Hoàn tất PDI
7. `preparation` — Đang chuẩn bị
8. `ready_for_delivery` — Sẵn sàng giao
9. `delivered` — Đã giao
10. `payment_completed` — Thanh toán đủ
11. `cancelled` — Đã hủy
12. `rental_active` — Đang thuê

### 8.5 VehicleUnit.cs
```
- Id (int)
- CarId (int) → Car
- VinNumber (string, unique) — Số khung
- EngineNumber (string, unique) — Số máy
- Color (string)
- InteriorColor (string)
- PurchasePrice (decimal) — Giá nhập
- ListPrice (decimal) — Giá niêm yết
- ImportDate (DateTime)
- CreatedById (int) → User
- Notes (string?)
- Status (string) — available/reserved/sold/service
```

### 8.6 PdiChecklist.cs (Pre-Delivery Inspection)
```
- Id (int)
- VehicleUnitId (int) → VehicleUnit
- InspectorId (int) → User
- ExteriorPass (bool) — Ngoại thất đạt
- InteriorPass (bool) — Nội thất đạt
- ElectricalPass (bool) — Hệ thống điện đạt
- EnginePass (bool) — Động cơ đạt
- TirePass (bool) — Lốp xe đạt
- Notes (string?)
- InspectedAt (DateTime)
- Defects → ICollection<PdiDefect>
```

### 8.7 PdiDefect.cs
```
- Id (int)
- PdiChecklistId (int) → PdiChecklist
- Category (string) — exterior/interior/electrical/engine/tire
- Reason (string) — Mô tả lỗi
- PhotoPath (string?) — Ảnh bằng chứng
```

### 8.8 PreDeliveryOrder.cs
```
- Id (int)
- OrderId (int) → Order
- VehicleUnitId (int) → VehicleUnit
- InstructionsJson (string) — JSON danh sách hướng dẫn
- Status (string) — pending/in_progress/completed
- AssignedToId (int?) → User
- Notes (string?)
- CreatedAt (DateTime)
- CompletedAt (DateTime?)
```

### 8.9 CustomerProfile.cs
```
- Id (int)
- UserId (int, unique) → User
- Source (string) — walk_in/online/referral
- InterestedModel (string?) — Xe quan tâm
- Phone (string?)
- Summary (string?) — Ghi chú tổng quan
- CreatedAt (DateTime)
- Notes → ICollection<CustomerNote>
```

### 8.10 CustomerNote.cs
```
- Id (int)
- CustomerProfileId (int) → CustomerProfile
- Content (string) — Nội dung ghi chú
- CreatedById (int) → User
- CreatedAt (DateTime)
```

### 8.11 TestDrive.cs
```
- Id (int)
- CustomerId (int) → User
- CarId (int) → Car
- LicensePlate (string?) — Biển số xe lái thử
- ScheduledDate (DateTime)
- ScheduledTime (TimeSpan)
- Status (string) — pending/confirmed/completed/cancelled
- Notes (string?)
- CreatedById (int) → User
- CreatedAt (DateTime)
```

### 8.12 PaymentReceipt.cs
```
- Id (int)
- ReceiptCode (string, unique) — Mã phiếu thu
- OrderId (int) → Order
- Amount (decimal)
- Type (string) — deposit/partial/full/service
- ConfirmedById (int?) → User
- Notes (string?)
- CreatedAt (DateTime)
```

### 8.13 PaymentVoucher.cs
```
- Id (int)
- VoucherCode (string, unique) — Mã phiếu chi
- Category (string) — Danh mục chi
- Amount (decimal)
- Recipient (string) — Người nhận
- Description (string)
- CreatedById (int) → User
- CreatedAt (DateTime)
```

### 8.14 Commission.cs
```
- Id (int)
- StaffId (int) → User
- OrderId (int) → Order
- Rate (decimal) — Tỷ lệ % hoa hồng
- Amount (decimal) — Số tiền hoa hồng
- Month (int)
- Year (int)
- IsPaid (bool)
- Notes (string?)
- CreatedAt (DateTime)
```

### 8.15 BankLoan.cs
```
- Id (int)
- OrderId (int) → Order
- BankName (string)
- LoanAmount (decimal)
- InterestRate (decimal) — %/năm
- LoanYears (int) — Số năm vay
- Status (string) — pending/approved/disbursed/rejected
- ConfirmedById (int?) → User
- Notes (string?)
- CreatedAt (DateTime)
```

### 8.16 ServiceTicket.cs
```
- Id (int)
- TicketCode (string, unique)
- LicensePlate (string) — Biển số xe vào xưởng
- CustomerName (string)
- CustomerPhone (string)
- Odometer (int) — Số km hiện tại
- Description (string) — Mô tả tình trạng
- Status (string) — received/assigned/in_progress/completed/paid
- TechnicianId (int?) → User
- LaborCost (decimal)
- PartsCost (decimal)
- TotalCost (decimal)
- CreatedById (int) → User
- ReceivedAt (DateTime)
- CompletedAt (DateTime?)
- SparePartUsages → ICollection<SparePartUsage>
```

### 8.17 SparePart.cs
```
- Id (int)
- PartCode (string, unique) — Mã phụ tùng
- Name (string)
- Stock (int) — Số lượng tồn
- MinStock (int) — Ngưỡng cảnh báo
- UnitPrice (decimal)
- Unit (string) — Đơn vị tính
- Usages → ICollection<SparePartUsage>
```

### 8.18 SparePartUsage.cs
```
- Id (int)
- ServiceTicketId (int) → ServiceTicket
- SparePartId (int) → SparePart
- Quantity (int)
- UnitPriceAtTime (decimal) — Giá tại thời điểm sử dụng
```

### 8.19 DiscountRequest.cs
```
- Id (int)
- OrderId (int) → Order
- CarPrice (decimal)
- DiscountAmount (decimal)
- Reason (string)
- Status (string) — pending/approved/rejected
- ReviewedById (int?) → User
- ReviewNotes (string?)
- CreatedAt (DateTime)
```

### 8.20 News.cs
```
- Id (int)
- Slug (string, unique)
- Title (string)
- Excerpt (string) — Tóm tắt
- Content (string) — Nội dung HTML
- ImageUrl (string?)
- Category (string)
- ReadTimeMinutes (int) — Thời gian đọc
- PublishedAt (DateTime)
- CreatedAt (DateTime)
```

### 8.21 TermsOfService.cs
```
- Id (int)
- Version (string) — e.g., "v1.0", "v2.0"
- Content (string) — Nội dung HTML
- EffectiveDate (DateTime)
- IsActive (bool)
- PublishedById (int) → User
- PublishedAt (DateTime)
- Agreements → ICollection<UserTermAgreement>
```

### 8.22 UserTermAgreement.cs
```
- UserId (int) → User [Composite PK]
- TermsOfServiceId (int) → TermsOfService [Composite PK]
- AgreedAt (DateTime)
- IpAddress (string?)
```

### 8.23 AuditLog.cs
```
- Id (int)
- UserId (int?)
- UserName (string?)
- Action (string) — Hành động
- Target (string?) — Đối tượng tác động
- Detail (string?) — Chi tiết
- CreatedAt (DateTime)
```

### 8.24 TodoItem.cs
```
- Id (int)
- Title (string)
- Description (string?)
- DueDate (DateTime?)
- Status (string) — pending/in_progress/completed
- AssignedToId (int?) → User
- CreatedAt (DateTime)
```

---

## 9. GIAO DIỆN NGƯỜI DÙNG (VIEWS)

### 9.1 Layout chính (_Layout.cshtml)
- Navbar cố định với scroll effect (trong suốt → solid)
- Menu: Trang chủ, Xe, Thuê xe, Ưu đãi, So sánh, Tin tức, Liên hệ
- Floating contact buttons: Zalo, điện thoại
- Footer với thông tin đại lý, mạng xã hội
- Responsive (mobile hamburger menu)
- Màu brand: Navy `#001C3D`, Đỏ `#D71920`

### 9.2 Layout Dashboard (_DashboardLayout.cshtml)
- Sidebar menu theo module (ẩn/hiện theo role)
- Top navbar với thông tin user đăng nhập
- Module: Tổng quan, Bán hàng, CRM, Kho, Kế toán, Dịch vụ, Admin

### 9.3 Views Khách hàng
| View | Đặc điểm |
|---|---|
| Home/Index | Hero slideshow (Ken Burns animation), featured cars grid, stats counter, news cards |
| Xe/Index | Filter sidebar + car grid, price range slider |
| Xe/Detail | Image gallery, specs table, features list, CTA buy/rent |
| Xe/Viewer3D | Full-screen 3D model viewer (GLB) |
| ThueXe/Index | Cards với giá/ngày, date picker |
| SoSanh/Index | 2 cột so sánh thông số |
| TinTuc/Index | Blog grid với category tags |
| UuDai/Index | Promotional cards |
| LienHe/Index | Form liên hệ với validation |

### 9.4 Views Authentication
| View | Đặc điểm |
|---|---|
| Account/Login | Form đơn giản, link quên mật khẩu |
| Account/Register | Form + agree terms checkbox |
| Account/VerifyCode | OTP 6 ô nhập số, countdown timer, resend |
| Account/ForgotPassword | Email input form |

### 9.5 Views Profile
| View | Đặc điểm |
|---|---|
| Profile/Index | Avatar, thông tin, form đổi mật khẩu |
| Profile/Orders | Table đơn hàng với status badges |

---

## 10. HỆ THỐNG XÁC THỰC VÀ PHÂN QUYỀN

### 10.1 Kiến trúc Auth
Dự án dùng **Dual Authentication**:
1. **Cookie-based Claims Authentication** (chính) — Lưu Claims vào cookie HTTP-only
2. **Session-based** (legacy support) — Lưu thông tin trong HttpContext.Session

### 10.2 Cấu hình (Program.cs)
```csharp
// Session: 8 giờ idle timeout, HTTP-only
services.AddSession(opts => {
    opts.IdleTimeout = TimeSpan.FromHours(8);
    opts.Cookie.HttpOnly = true;
});

// Cookie Auth: 8 giờ sliding expiry
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts => {
        opts.ExpireTimeSpan = TimeSpan.FromHours(8);
        opts.SlidingExpiration = true;
        opts.LoginPath = "/Account/Login";
    });
```

### 10.3 Claims được lưu trong Cookie
```csharp
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
new Claim(ClaimTypes.Email, user.Email)
new Claim(ClaimTypes.Name, user.Name)
new Claim(ClaimTypes.Role, user.Role)
```

### 10.4 Roles và quyền truy cập
| Role | Mô tả | Dashboard access |
|---|---|---|
| `admin` | Quản trị viên | Full access + TermsPolicy |
| `sale` | Nhân viên bán hàng | Sales, CRM, Orders |
| `warehouse` | Nhân viên kho | Xe, VehicleUnit, PDI, PreDelivery |
| `accounting` | Kế toán | PaymentReceipt, Voucher, Commission, Loan |
| `service` | Kỹ thuật viên | ServiceTicket, SparePart |
| `customer` | Khách hàng | Profile, Orders (public pages) |

### 10.5 Luồng Đăng ký
```
Register (email, password, name)
→ Validate email format + MX record (DnsClient)
→ BCrypt hash password
→ Create User (IsEmailVerified = false)
→ Generate 6-digit OTP + 15min expiry
→ Send OTP email (MailKit)
→ Redirect to VerifyCode page
→ User enters OTP
→ Set IsEmailVerified = true
→ Login automatically
```

### 10.6 Luồng Đăng nhập
```
Login (email, password)
→ Find User by email
→ BCrypt verify password
→ Check IsActive
→ Check IsEmailVerified
→ Create ClaimsPrincipal
→ SignIn with Cookie
→ Set Session data
→ Check terms acceptance (UserTermAgreement)
→ If new terms → AcceptNewTerms page
→ If role == customer → /
→ If role == staff/admin → /Dashboard
```

### 10.7 Xác thực Email
- **OTP:** 6 chữ số, 15 phút, lưu trong User.EmailVerificationCode + Expiry
- **Token link:** Secure random token, 24 giờ, lưu trong User.EmailVerificationToken
- **Gửi lại:** Cho phép resend sau khi OTP hết hạn

---

## 11. DỮ LIỆU MẪU (SEED DATA)

### 11.1 Tài khoản mặc định
| Email | Mật khẩu | Role |
|---|---|---|
| admin@autoht.vn | Admin@123 | admin |
| sale@autoht.vn | Staff@123 | sale |
| kho@autoht.vn | Staff@123 | warehouse |
| ketoan@autoht.vn | Staff@123 | accounting |
| kythuat@autoht.vn | Staff@123 | service |
| khachhang1@gmail.com | 123456 | customer |
| ... (đến khachhang20) | 123456 | customer |

### 11.2 Xe mẫu (40+ xe)
**Mercedes-Benz (11 model):**
- E 300 AMG, C 300 AMG, S 500, GLC 300, GLE 450
- AMG GT 63 S, Maybach S 580, EQS 580, G 63 AMG, CLA 250, A 200

**BMW:** 3 Series, 5 Series, X5, M3 Competition

**Audi:** A4, Q5, e-tron

**Toyota:** Camry, Land Cruiser

**Honda:** CR-V, Civic Type R

**VinFast:** VF3, VF5, VF8, VF9

**Siêu xe:** Lamborghini Urus, Ferrari Roma, McLaren P1

**Giá từ:** 240 triệu (VF3) → 30 tỷ (McLaren P1)

### 11.3 Dữ liệu quan hệ
- 20 CustomerProfile (tương ứng 20 khách hàng)
- 60 CustomerNote (3 ghi chú/khách)
- 3-5 TestDrive ở các trạng thái khác nhau
- 11 Order (buy + rent, đủ trạng thái)
- 11 VehicleUnit (VIN + số máy)
- 11 PdiChecklist (1/xe đơn vị)
- 3-5 PreDeliveryOrder
- Phiếu thu, phiếu chi, hoa hồng mẫu
- 1 BankLoan mẫu
- 7 SparePart (nhớt, lọc, má phanh...)
- 3 ServiceTicket
- 1 TermsOfService (v1.0)

---

## 12. BẢO MẬT

### 12.1 Mật khẩu
- Hash bằng **BCrypt** với salt ngẫu nhiên
- BCrypt.Net-Next 4.0.3 — work factor mặc định

### 12.2 CSRF Protection
- `[ValidateAntiForgeryToken]` trên tất cả POST actions
- Razor tự thêm `@Html.AntiForgeryToken()` qua tag helper `asp-action`

### 12.3 Cookie Security
- `HttpOnly = true` — Chống XSS đọc cookie
- `SameSite = Lax` — Chống CSRF cơ bản
- Sliding expiration — Giảm window tấn công

### 12.4 Input Validation
- Data Annotations trên Models
- Client-side: jQuery Validation + Unobtrusive
- Server-side: ModelState.IsValid
- Email: Regex + MX record check (DnsClient) + Gmail-specific rules

### 12.5 Authorization
- `[Authorize]` — Yêu cầu đăng nhập
- `[Authorize(Roles="admin")]` — Chỉ admin (TermsPolicyController)
- Role-based sidebar visibility — Ẩn menu theo role

### 12.6 File Upload Security
- Giới hạn 30MB (Kestrel MaxRequestBodySize)
- Lưu UUID filename (không dùng tên gốc)
- Chỉ lưu trong `wwwroot/images/cars/` (kiểm soát được)

---

## 13. TÍNH NĂNG ĐẶC BIỆT

### 13.1 3D Car Viewer
- Định dạng: `.glb` (GL Transmission Format Binary)
- 40+ model 3D của các xe sang
- Custom MIME type trong Program.cs
- Route: `/xe/{slug}/3d`
- Render bằng Three.js (embedded viewer)

### 13.2 AJAX Search Suggestions
- Endpoint: `GET /Xe/SearchSuggest?q={term}`
- Trả về JSON array 6 xe phù hợp
- Hiển thị real-time khi gõ (debounce)

### 13.3 Email Verification
- Double verification: OTP + link token
- MX record check — đảm bảo domain email tồn tại
- Gmail-specific rules — Ngăn địa chỉ không hợp lệ

### 13.4 Multi-version Terms of Service
- Tạo phiên bản mới → tự động vô hiệu phiên bản cũ
- Track user agreement theo IP
- Buộc chấp nhận khi phiên bản mới được publish

### 13.5 Car Comparison
- So sánh side-by-side 2 xe bất kỳ
- Highlight điểm khác biệt
- Deep-link bằng slug: `/SoSanh?xe1=slug1&xe2=slug2`

### 13.6 PDI Workflow
```
VehicleUnit được tạo
→ Tạo PdiChecklist
→ Inspector kiểm tra 5 hạng mục
→ Ghi nhận PdiDefect nếu có lỗi
→ Đánh dấu PDI hoàn tất
→ Tạo PreDeliveryOrder
→ Chuẩn bị xe (wash, fuel, accessories)
→ Bàn giao cho khách
```

### 13.7 Quy trình Order (12 trạng thái)
```
pending → confirmed → deposit_paid → vehicle_assigned 
→ pdi_pending → pdi_completed → preparation 
→ ready_for_delivery → delivered → payment_completed
(hoặc cancelled bất kỳ lúc nào)
(thuê xe: rental_active → ...)
```

---

## 14. PHÂN TÍCH CHI TIẾT TỪNG MODULE

### 14.1 Module Trang chủ (Home)
**Mục đích:** Landing page thu hút, giới thiệu thương hiệu và sản phẩm nổi bật.

**Thành phần:**
- **Hero Section:** Slideshow 3-5 ảnh với hiệu ứng Ken Burns (zoom slow), overlay text, CTA button
- **Featured Cars:** Grid 4-6 xe nổi bật (status = active, sắp xếp mới nhất)
- **Stats Counter:** Số xe, khách hàng, năm kinh nghiệm, showroom
- **Latest News:** 3 bài tin tức mới nhất
- **Brand Showcase:** Logo các hãng xe đang có
- **CTA Section:** Khuyến khích đặt lái thử hoặc liên hệ tư vấn

**Data queries:**
```
Cars: status="active", order by CreatedAt desc, take 6
News: order by PublishedAt desc, take 3
Terms: IsActive = true (để hiển thị nếu cần)
```

### 14.2 Module Danh mục xe (Xe/Index)
**Bộ lọc:**
- Hãng xe (brand): Dropdown hoặc checkbox
- Loại xe (type): Sedan, SUV, Coupe, Pickup...
- Nhiên liệu (fuel): Xăng, Diesel, Điện, Hybrid
- Tìm kiếm tên: LIKE query
- Khoảng giá: min-max slider
- Loại giao dịch: Mua / Thuê

**Kết quả:** Paginated grid, mỗi card gồm:
- Ảnh đầu tiên từ ImagesJson
- Tên, hãng, năm
- Giá (hoặc giá/ngày nếu thuê)
- Badge loại xe, nhiên liệu
- Nút Xem chi tiết

### 14.3 Module Chi tiết xe (Xe/Detail)
**Nội dung:**
- Gallery ảnh (carousel)
- Thông số kỹ thuật (từ SpecsJson): động cơ, hộp số, công suất, mô-men xoắn, kích thước...
- Tính năng (từ FeaturesJson): an toàn, tiện nghi, công nghệ...
- Giá và CTA: Mua ngay / Thuê ngay / Liên hệ
- 3D Viewer link (nếu có Model3dUrl)
- Xe liên quan: Cùng hãng, loại tương tự

**Form đặt mua (DatMua):**
- Loại: Mua / Thuê
- Nếu thuê: Chọn ngày bắt đầu và kết thúc
- Ghi chú
- Submit → POST `/Xe/DatMua` → Tạo Order

### 14.4 Module Quản lý đơn hàng (Dashboard/DonHang)
**Luồng xử lý đơn:**
1. **pending** — Đơn mới tạo, chờ sale xác nhận
2. **confirmed** — Sale đã xác nhận, chờ khách đặt cọc
3. **deposit_paid** — Kế toán xác nhận nhận cọc
4. **vehicle_assigned** — Kho phân xe cụ thể (VehicleUnit)
5. **pdi_pending** — Giao cho kỹ thuật làm PDI
6. **pdi_completed** — PDI xong, không có lỗi nghiêm trọng
7. **preparation** — Chuẩn bị xe (vệ sinh, đổ xăng, phụ kiện)
8. **ready_for_delivery** — Xe sẵn sàng giao
9. **delivered** — Đã bàn giao cho khách
10. **payment_completed** — Thanh toán đầy đủ (kế toán xác nhận)
11. **cancelled** — Hủy
12. **rental_active** — Đang trong thời gian thuê

### 14.5 Module PDI (Kiểm tra trước giao xe)
**5 hạng mục kiểm tra:**
1. **Ngoại thất:** Sơn, kính, đèn, gương, bánh
2. **Nội thất:** Ghế, taplo, âm thanh, điều hòa
3. **Hệ thống điện:** Đèn, màn hình, cảm biến, camera
4. **Động cơ:** Dầu, nước, đai, rò rỉ
5. **Lốp xe:** Áp suất, gai lốp, mâm

**Nếu fail:** Tạo PdiDefect với:
- Category (thuộc hạng mục nào)
- Reason (mô tả lỗi)
- PhotoPath (ảnh chụp lỗi)

**Kết quả:**
- Pass tất cả → Cập nhật Order → pdi_completed
- Fail → Ghi nhận defect → Sửa → PDI lại

### 14.6 Module Dịch vụ (Service)
**Luồng xử lý phiếu dịch vụ:**
```
received → assigned (phân kỹ thuật viên) 
→ in_progress (đang sửa)
→ completed (hoàn tất)
→ paid (khách thanh toán)
```

**Tính chi phí:**
- Labor cost: Công lao động (nhập thủ công)
- Parts cost: Tự tính từ SparePartUsage (quantity × unitPriceAtTime)
- Total = Labor + Parts

**Kho phụ tùng:**
- Cảnh báo khi Stock < MinStock (hiển thị badge đỏ)
- Trừ tồn kho khi thêm SparePartUsage
- Lưu giá tại thời điểm sử dụng (tránh sai lệch khi giá thay đổi)

### 14.7 Module Kế toán (Accounting)
**PaymentReceipt (Phiếu thu):**
- Mã tự sinh (e.g., PT-20260521-001)
- 4 loại: deposit, partial, full, service
- Liên kết với Order
- Kế toán xác nhận và ký tên

**PaymentVoucher (Phiếu chi):**
- Mã tự sinh (e.g., PC-20260521-001)
- Danh mục: lương, marketing, vận chuyển, khác
- Người nhận và mô tả

**Commission (Hoa hồng):**
- Tính theo % tổng giá trị đơn
- Group theo tháng/năm
- Đánh dấu đã chi trả

### 14.8 Module CRM
**CustomerProfile:**
- Tự động tạo khi user đặt hàng lần đầu
- Source tracking: walk-in, online, referral (giới thiệu)
- Xe quan tâm để tư vấn trúng hướng

**CustomerNote (Timeline):**
- Nhân viên ghi lại mỗi lần liên hệ
- Thời gian và người ghi được lưu
- Hiển thị dạng timeline, mới nhất trên cùng

**TestDrive:**
- Khách đặt lái thử qua web hoặc sale tạo
- Sale xác nhận → thông báo ngày giờ
- Ghi nhận kết quả: quan tâm / không quan tâm

---

## 15. CẤU HÌNH HỆ THỐNG (Program.cs & appsettings.json)

### 15.1 Program.cs highlights
```csharp
// File upload limit
builder.WebHost.ConfigureKestrel(opt => {
    opt.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500MB Kestrel
});
// (Controller limit: 30MB per action)

// Database: SQL Server với auto-migration khi start
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(connectionString));

// Auto-migrate + auto-seed on startup
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db); // Seed nếu chưa có dữ liệu
}

// Custom MIME types cho 3D models
provider.AddMapping(".glb", "model/gltf-binary");
provider.AddMapping(".gltf", "model/gltf+json");

// Custom routes
routes.MapControllerRoute("xe-detail", "xe/{slug}", 
    new { controller = "Xe", action = "Detail" });
routes.MapControllerRoute("xe-3d", "xe/{slug}/3d",
    new { controller = "Xe", action = "Viewer3D" });
routes.MapControllerRoute("tin-tuc", "tin-tuc/{slug}",
    new { controller = "TinTuc", action = "Detail" });
```

### 15.2 appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AutoEliteDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "thaiduong162000@gmail.com",
    "SenderName": "AutoHT",
    "EnableSsl": true
  },
  "AppSettings": {
    "SiteName": "AutoHT"
  }
}
```

---

## 16. FRONTEND STACK CHI TIẾT

### 16.1 CSS Framework
- **Tailwind CSS** (CDN) — Framework chính, utility-first
- **Bootstrap** — Grid system, một số components
- **Custom CSS** (`wwwroot/css/site.css`) — Override và animation

### 16.2 JavaScript
- **jQuery 3.6+** — DOM, AJAX, events
- **jQuery Validation** — Client-side form validation
- **jQuery Validation Unobtrusive** — ASP.NET integration
- **Alpine.js** — Reactive dropdown, modal (inline minimal)
- **Custom `wwwroot/js/site.js`** — Slideshow, animations, form handling

### 16.3 Animations & UX
- **Ken Burns effect** — Zoom chậm hero images
- **FadeInUp** — Cards xuất hiện khi scroll
- **Smooth scroll** — Navigation internal links
- **Loading spinner** — Nút submit khi xử lý
- **Toast notifications** — TempData["Success"] / TempData["Error"]
- **Scroll-aware navbar** — Transparent → solid khi scroll

### 16.4 Colors (Brand)
```css
--navy: #001C3D;   /* Màu chính, navbar, header */
--red: #D71920;    /* Accent, CTA buttons, badges */
--gold: #C8A55A;   /* Premium highlight */
--white: #FFFFFF;
--gray-light: #F5F5F5;
```

---

## 17. ĐIỂM NỔI BẬT KỸ THUẬT

1. **Dual Auth System:** Cookie Claims + Session chạy song song
2. **Email MX Validation:** Dùng DnsClient để verify domain email thực sự tồn tại
3. **3D Model Support:** Custom MIME + GLB viewer tích hợp
4. **JSON Column Storage:** Xe specs/features lưu dạng JSON trong một column → flexible schema
5. **Comprehensive Seeding:** 40+ xe, 20 khách hàng, đủ scenarios để demo
6. **Multi-module Dashboard:** 5 phân hệ, sidebar responsive theo role
7. **Terms Versioning:** Phiên bản điều khoản với user acceptance tracking
8. **Financial Precision:** decimal(18,2) cho tất cả giá trị tài chính
9. **Slug-based URLs:** SEO-friendly cho xe và bài viết
10. **Audit Trail:** AuditLog ghi lại hành động người dùng

---

*File tài liệu này được tạo tự động từ phân tích mã nguồn dự án. Cập nhật lần cuối: 2026-05-21*
