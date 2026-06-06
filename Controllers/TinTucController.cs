using CarWebsite.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

public class TinTucController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var news = await db.News.OrderByDescending(n => n.PublishedAt).ToListAsync();
        return View(news);
    }

    public async Task<IActionResult> Detail(string slug)
    {
        var article = await db.News.FirstOrDefaultAsync(n => n.Slug == slug);
        if (article == null) return NotFound();
        return View(article);
    }
}
