namespace CarWebsite.Models;

// Phụ tùng sử dụng trong 1 phiếu dịch vụ
public class SparePartUsage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ServiceTicketId { get; set; }
    public ServiceTicket ServiceTicket { get; set; } = null!;

    public Guid SparePartId { get; set; }
    public SparePart SparePart { get; set; } = null!;

    public int Quantity { get; set; }

    public long UnitPrice { get; set; } // Giá tại thời điểm sử dụng

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
