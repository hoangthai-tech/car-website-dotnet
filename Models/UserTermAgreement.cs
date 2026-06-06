namespace CarWebsite.Models;

public class UserTermAgreement
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int TermsOfServiceId { get; set; }
    public TermsOfService TermsOfService { get; set; } = null!;

    public DateTime AgreedAt { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }
}
