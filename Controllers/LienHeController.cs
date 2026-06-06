using CarWebsite.Data;
using CarWebsite.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarWebsite.Controllers;

public class LienHeController(AppDbContext db) : Controller
{
    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string name, string phone, string email, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(message))
        {
            TempData["Error"] = "Vui lòng điền đầy đủ họ tên và nội dung.";
            return RedirectToAction("Index");
        }

        db.AuditLogs.Add(new AuditLog
        {
            UserName = name,
            Action = "Liên hệ mới",
            Target = $"{phone} | {email}",
            Detail = message
        });
        await db.SaveChangesAsync();

        TempData["Success"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong vòng 30 phút.";
        return RedirectToAction("Index");
    }
}
