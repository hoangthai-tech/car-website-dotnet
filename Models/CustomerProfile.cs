using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

public class CustomerProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    // walk-in | online | referral | event | other
    public string Source { get; set; } = "other";

    public string? InterestedCarModel { get; set; } // Dòng xe quan tâm

    public string? Phone { get; set; }

    public string? Summary { get; set; } // Ghi chú tổng quan nội bộ

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CustomerNote> ContactHistory { get; set; } = [];
}
