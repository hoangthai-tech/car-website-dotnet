using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWebsite.Models;

public class Car
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Slug { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string Brand { get; set; } = "";

    [Required]
    public string Type { get; set; } = ""; // Sedan | SUV | Coupe | Pickup | Hatchback

    [Required]
    public string Fuel { get; set; } = ""; // Xăng | Điện | Hybrid

    public long Price { get; set; }
    public string PriceDisplay { get; set; } = "";
    public int Year { get; set; } = 2024;

    public string Image { get; set; } = "";
    public string ImagesJson { get; set; } = "[]"; // JSON array

    public string? Badge { get; set; }
    public string? Model3DUrl { get; set; }

    public string SpecsJson { get; set; } = "{}"; // JSON object
    public string FeaturesJson { get; set; } = "[]"; // JSON array

    public long RentalPricePerDay { get; set; }
    public int Stock { get; set; } = 1;

    public string Status { get; set; } = "pending"; // approved | pending | draft

    public Guid? CreatedById { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public List<string> Images
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(ImagesJson) ?? [];
        set => ImagesJson = System.Text.Json.JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public Dictionary<string, string> Specs
    {
        get => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(SpecsJson) ?? [];
        set => SpecsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }

    [NotMapped]
    public List<string> Features
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(FeaturesJson) ?? [];
        set => FeaturesJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
}
