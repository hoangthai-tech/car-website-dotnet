using CarWebsite.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

public class UuDaiController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var rentalCars = await db.Cars
            .Where(c => c.Status == "approved" && c.RentalPricePerDay > 0)
            .OrderBy(c => c.RentalPricePerDay)
            .Take(3)
            .ToListAsync();

        ViewBag.RentalCars = rentalCars;
        return View();
    }
}
