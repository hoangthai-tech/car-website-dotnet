using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string OrderCode { get; set; } = "";

    public Guid? CustomerId { get; set; }
    public User? Customer { get; set; }

    public Guid? CarId { get; set; }

    public Guid? VehicleUnitId { get; set; } // Xe cụ thể (từng chiếc) đã đặt cọc

    public long DepositAmount { get; set; } // Số tiền cọc

    [Required]
    public string CarName { get; set; } = "";

    [Required]
    public string CustomerName { get; set; } = "";

    public string OrderType { get; set; } = "buy"; // "buy" | "rent"

    public long Amount { get; set; }
    public long RentalDailyRate { get; set; }

    public DateTime? RentalStartDate { get; set; }
    public DateTime? RentalEndDate { get; set; }

    public string Status { get; set; } = "Khách đang xem xe";

    public Guid? StaffId { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
