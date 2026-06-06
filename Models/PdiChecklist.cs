using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWebsite.Models;

// Phiếu kiểm tra chất lượng nhập kho (Pre-Delivery Inspection)
public class PdiChecklist
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VehicleUnitId { get; set; }
    public VehicleUnit VehicleUnit { get; set; } = null!;

    public bool ExteriorPassed { get; set; }   // Ngoại thất
    public bool InteriorPassed { get; set; }   // Nội thất
    public bool ElectricalPassed { get; set; } // Hệ thống điện
    public bool EnginePassed { get; set; }     // Động cơ
    public bool TirePassed { get; set; }       // Lốp xe

    [NotMapped]
    public bool OverallPassed =>
        ExteriorPassed && InteriorPassed && ElectricalPassed && EnginePassed && TirePassed;

    public string? Notes { get; set; }

    public Guid InspectorId { get; set; }
    public User Inspector { get; set; } = null!;

    public DateTime InspectedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PdiDefect> Defects { get; set; } = [];
}
