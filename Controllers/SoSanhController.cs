using CarWebsite.Data;
using CarWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

public class SoSanhController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? slug1, string? slug2, string? slug3)
    {
        var slugs = new[] { slug1, slug2, slug3 }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        var allCars = await db.Cars
            .Where(c => c.Status == "approved" && slugs.Contains(c.Slug))
            .ToListAsync();

        // giữ đúng thứ tự slug1, slug2, slug3
        var cars = slugs
            .Select(s => allCars.FirstOrDefault(c => c.Slug == s))
            .Where(c => c != null)
            .Cast<Car>()
            .ToList();

        return View(cars);
    }
}
