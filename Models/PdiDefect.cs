using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

// Lỗi phát hiện trong quá trình PDI
public class PdiDefect
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PdiChecklistId { get; set; }
    public PdiChecklist PdiChecklist { get; set; } = null!;

    // exterior | interior | electrical | engine | tire
    [Required]
    public string Category { get; set; } = "";

    [Required]
    public string Reason { get; set; } = "";

    public string? PhotoPath { get; set; } // Đường dẫn ảnh lỗi upload

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
