using System.Diagnostics;
using CarWebsite.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var featuredCars = await db.Cars
            .Where(c => c.Status == "approved")
            .OrderByDescending(c => c.Price)
            .Take(6)
            .ToListAsync();

        var heroCars = featuredCars.Take(5).ToList();

        var latestNews = await db.News
            .OrderByDescending(n => n.PublishedAt)
            .Take(3)
            .ToListAsync();

        ViewBag.FeaturedCars = featuredCars;
        ViewBag.HeroCars = heroCars;
        ViewBag.LatestNews = latestNews;
        return View();
    }

    public async Task<IActionResult> Terms()
    {
        var activeTerms = await db.TermsOfServices
            .Where(t => t.IsActive)
            .FirstOrDefaultAsync();
        return View(activeTerms);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
