using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

// Phiếu dịch vụ bảo dưỡng / sửa chữa
public class ServiceTicket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string TicketCode { get; set; } = ""; // DV-YYYYMMDD-XXXX

    [Required]
    public string LicensePlate { get; set; } = ""; // Biển số xe khách

    [Required]
    public string CustomerName { get; set; } = "";

    public string CustomerPhone { get; set; } = "";

    public int Odometer { get; set; } // Số km đã đi

    [Required]
    public string Description { get; set; } = ""; // Tình trạng / yêu cầu sửa chữa

    // received | assigned | inprogress | completed | paid
    public string Status { get; set; } = "received";

    public Guid? AssignedTechnicianId { get; set; }
    public User? AssignedTechnician { get; set; }

    public long LaborCost { get; set; } // Tiền công thợ

    public long TotalPartsCost { get; set; } // Tổng tiền phụ tùng (tính tự động)

    public long TotalAmount { get; set; } // = LaborCost + TotalPartsCost

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ICollection<SparePartUsage> SparePartUsages { get; set; } = [];
}
