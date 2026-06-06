using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

// Công nợ ngân hàng - hồ sơ mua trả góp
public class BankLoan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [Required]
    public string BankName { get; set; } = "";

    public long LoanAmount { get; set; }

    public decimal InterestRate { get; set; } // %/năm

    public int LoanYears { get; set; }

    // pending | disbursed | rejected
    public string Status { get; set; } = "pending";

    public DateTime? DisbursedAt { get; set; }

    public Guid? ConfirmedById { get; set; }
    public User? ConfirmedBy { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
