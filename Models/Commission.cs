namespace CarWebsite.Models;

// Hoa hồng nhân viên kinh doanh
public class Commission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StaffId { get; set; }
    public User Staff { get; set; } = null!;

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal Rate { get; set; } = 0.01m; // 1% mặc định

    public long Amount { get; set; } // = Order.Amount * Rate

    public int Month { get; set; }

    public int Year { get; set; }

    public bool IsPaid { get; set; } = false;

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
