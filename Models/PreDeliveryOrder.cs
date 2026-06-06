using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

// Lệnh chuẩn bị xe giao cho khách
public class PreDeliveryOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid VehicleUnitId { get; set; }
    public VehicleUnit VehicleUnit { get; set; } = null!;

    // JSON array: ["Rửa xe", "Lắp phim cách nhiệt", "Lắp camera hành trình"]
    public string InstructionsJson { get; set; } = "[]";

    // pending | inprogress | done
    public string Status { get; set; } = "pending";

    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
