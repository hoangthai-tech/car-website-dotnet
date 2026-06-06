using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

public class TermsOfService
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string Version { get; set; } = "";       // "v1.0", "v2.0"

    [Required]
    public string Content { get; set; } = "";        // Nội dung HTML

    public DateTime EffectiveDate { get; set; }      // Ngày hiệu lực

    public bool IsActive { get; set; } = false;      // Phiên bản đang áp dụng

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public Guid? PublishedById { get; set; }
    public User? PublishedBy { get; set; }

    public ICollection<UserTermAgreement> Agreements { get; set; } = [];
}
