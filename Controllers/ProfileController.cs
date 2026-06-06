using CarWebsite.Data;
using CarWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

public class ProfileController(AppDbContext db) : Controller
{
    private Guid? CurrentUserId()
    {
        var id = HttpContext.Session.GetString("UserId");
        return Guid.TryParse(id, out var uid) ? uid : null;
    }

    public async Task<IActionResult> Index()
    {
        var uid = CurrentUserId();
        if (uid == null) return RedirectToAction("Login", "Account");

        var user = await db.Users.FindAsync(uid);
        if (user == null) return RedirectToAction("Logout", "Account");
        return View(user);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string name)
    {
        var uid = CurrentUserId();
        if (uid == null) return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Họ tên không được để trống.";
            return RedirectToAction("Index");
        }

        var user = await db.Users.FindAsync(uid);
        if (user == null) return RedirectToAction("Logout", "Account");

        user.Name = name.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        HttpContext.Session.SetString("UserName", user.Name);
        TempData["Success"] = "Cập nhật thông tin thành công.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var uid = CurrentUserId();
        if (uid == null) return RedirectToAction("Login", "Account");

        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "Mật khẩu mới không khớp.";
            return RedirectToAction("Index");
        }
        if (newPassword.Length < 8)
        {
            TempData["Error"] = "Mật khẩu mới phải có ít nhất 8 ký tự.";
            return RedirectToAction("Index");
        }

        var user = await db.Users.FindAsync(uid);
        if (user == null) return RedirectToAction("Logout", "Account");

        if (user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            TempData["Error"] = "Mật khẩu hiện tại không đúng.";
            return RedirectToAction("Index");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Orders()
    {
        var uid = CurrentUserId();
        if (uid == null) return RedirectToAction("Login", "Account");

        var orders = await db.Orders
            .Where(o => o.CustomerId == uid)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return View(orders);
    }
}
