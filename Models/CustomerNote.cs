using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

// Dòng thời gian tư vấn CRM
public class CustomerNote
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    [Required]
    public string Content { get; set; } = "";

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
