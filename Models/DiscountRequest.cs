namespace CarWebsite.Models;

public class DiscountRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? OrderId { get; set; }
    public string CarName { get; set; } = "";
    public long CarPrice { get; set; }
    public long Discount { get; set; }
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "pending"; // pending | approved | rejected
    public Guid? StaffId { get; set; }
    public string StaffName { get; set; } = "";
    public Guid? ReviewedById { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
