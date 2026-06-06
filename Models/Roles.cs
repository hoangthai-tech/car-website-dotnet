namespace CarWebsite.Models;

/// <summary>
/// Hằng số tên Role dùng thống nhất toàn dự án.
/// </summary>
public static class Roles
{
    public const string Admin       = "admin";
    public const string Sale        = "sale";        // Kinh doanh
    public const string Warehouse   = "warehouse";   // Kho
    public const string Accounting  = "accounting";  // Kế toán
    public const string Service     = "service";     // Kỹ thuật & Hậu mãi
    public const string Customer    = "customer";    // Khách hàng

    // Tương thích ngược với hệ thống cũ
    public const string Manager     = "manager";
    public const string Staff       = "staff";

    /// <summary>Tất cả các role nhân viên (không phải khách hàng).</summary>
    public static readonly string[] AllStaff = [Admin, Sale, Warehouse, Accounting, Service, Manager, Staff];

    /// <summary>Hiển thị tên thân thiện cho từng role.</summary>
    public static string DisplayName(string role) => role switch
    {
        Admin      => "Quản trị viên",
        Sale       => "Kinh doanh",
        Warehouse  => "Kho",
        Accounting => "Kế toán",
        Service    => "Kỹ thuật & Hậu mãi",
        Manager    => "Quản lý",
        Staff      => "Nhân viên",
        Customer   => "Khách hàng",
        _          => role
    };

    public static string BadgeColor(string role) => role switch
    {
        Admin      => "bg-red-100 text-red-700",
        Sale       => "bg-blue-100 text-blue-700",
        Warehouse  => "bg-amber-100 text-amber-700",
        Accounting => "bg-green-100 text-green-700",
        Service    => "bg-purple-100 text-purple-700",
        Manager    => "bg-indigo-100 text-indigo-700",
        Staff      => "bg-gray-100 text-gray-700",
        _          => "bg-gray-100 text-gray-600"
    };
}
