using CarWebsite.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

public class ThueXeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var cars = await db.Cars
            .Where(c => c.Status == "approved" && c.RentalPricePerDay > 0)
            .OrderBy(c => c.RentalPricePerDay)
            .ToListAsync();
        return View(cars);
    }
}
