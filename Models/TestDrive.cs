using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

public class TestDrive
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public Guid CarId { get; set; }
    public Car Car { get; set; } = null!;

    [Required]
    public string LicensePlate { get; set; } = ""; // Biển số xe lái thử

    public DateTime ScheduledDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    // pending | confirmed | completed | cancelled
    public string Status { get; set; } = "pending";

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
