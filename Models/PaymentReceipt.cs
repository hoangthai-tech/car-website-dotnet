using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

// Phiếu thu tiền
public class PaymentReceipt
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string ReceiptCode { get; set; } = ""; // PT-YYYYMMDD-XXXX

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public long Amount { get; set; }

    // deposit | partial | full | service
    public string PaymentType { get; set; } = "deposit";

    public Guid ConfirmedById { get; set; }
    public User ConfirmedBy { get; set; } = null!;

    public DateTime ConfirmedAt { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }
}
