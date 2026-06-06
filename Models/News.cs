using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

public class News
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Slug { get; set; } = "";

    [Required]
    public string Title { get; set; } = "";

    public string Excerpt { get; set; } = "";
    public string Content { get; set; } = "";
    public string Image { get; set; } = "";
    public string Category { get; set; } = "";
    public string ReadTime { get; set; } = "";
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}
