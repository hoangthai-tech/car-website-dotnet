using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

// Phiếu chi nội bộ
public class PaymentVoucher
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string VoucherCode { get; set; } = ""; // PC-YYYYMMDD-XXXX

    // car_import | deposit_refund | salary | commission | operation
    public string Category { get; set; } = "operation";

    public long Amount { get; set; }

    [Required]
    public string Recipient { get; set; } = ""; // Người/đơn vị nhận tiền

    [Required]
    public string Description { get; set; } = "";

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
