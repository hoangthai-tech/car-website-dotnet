using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

public class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    // pending | inprogress | done
    public string Status { get; set; } = "pending";

    public Guid AssignedToId { get; set; }
    public User AssignedTo { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
