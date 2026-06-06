using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

public class VehicleUnit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Vin { get; set; } = ""; // Số khung (VIN)

    [Required]
    public string EngineNumber { get; set; } = ""; // Số máy

    public Guid CarId { get; set; }
    public Car Car { get; set; } = null!;

    [Required]
    public string ExteriorColor { get; set; } = ""; // Màu ngoại thất

    public string InteriorColor { get; set; } = ""; // Màu nội thất

    public long PurchasePrice { get; set; } // Giá nhập

    public long ListPrice { get; set; } // Giá bán niêm yết (0 = dùng giá từ Car)

    // available | reserved | repair | sold
    public string Status { get; set; } = "available";

    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PdiChecklist> PdiChecklists { get; set; } = [];
}
