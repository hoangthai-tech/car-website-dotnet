using System.ComponentModel.DataAnnotations;

namespace CarWebsite.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    public string? PasswordHash { get; set; }

    [Required]
    public string Role { get; set; } = "customer"; // admin | manager | staff | customer

    public string Branch { get; set; } = "TP. Hồ Chí Minh";

    public string? Avatar { get; set; }

    public bool EmailVerified { get; set; } = false;
    public string? VerificationToken { get; set; }
    public DateTime? VerificationExpiry { get; set; }

    public bool IsActive { get; set; } = true; // Khóa/Mở khóa tài khoản

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = [];
}
