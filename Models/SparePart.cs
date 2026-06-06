using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWebsite.Models;

public class SparePart
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string PartCode { get; set; } = ""; // Mã phụ tùng

    [Required]
    public string Name { get; set; } = ""; // Tên phụ tùng

    public int Stock { get; set; } // Số lượng tồn kho

    public int MinStock { get; set; } = 5; // Mức tối thiểu cần có

    public long UnitPrice { get; set; } // Giá bán

    public string Unit { get; set; } = "cai"; // cai | lit | bo | cuon

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool IsLowStock => Stock <= MinStock;

    public ICollection<SparePartUsage> Usages { get; set; } = [];
}
