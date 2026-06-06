using CarWebsite.Data;
using CarWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

[Authorize(Roles = "admin")]
public class TermsPolicyController(AppDbContext db) : Controller
{
    // GET: /TermsPolicy — Danh sách phiên bản
    public async Task<IActionResult> Index()
    {
        var list = await db.TermsOfServices
            .Include(t => t.PublishedBy)
            .OrderByDescending(t => t.PublishedAt)
            .ToListAsync();
        return View(list);
    }

    // POST: /TermsPolicy/Publish — Xuất bản phiên bản mới
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(string version, string content, DateTime effectiveDate)
    {
        if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ Số phiên bản và Nội dung điều khoản.";
            return RedirectToAction(nameof(Index));
        }

        if (await db.TermsOfServices.AnyAsync(t => t.Version == version))
        {
            TempData["Error"] = $"Phiên bản '{version}' đã tồn tại. Vui lòng dùng số phiên bản khác.";
            return RedirectToAction(nameof(Index));
        }

        // Huỷ active phiên bản cũ
        await db.TermsOfServices
            .Where(t => t.IsActive)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));

        var adminId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
        var terms = new TermsOfService
        {
            Version       = version.Trim(),
            Content       = content.Trim(),
            EffectiveDate = effectiveDate,
            IsActive      = true,
            PublishedAt   = DateTime.UtcNow,
            PublishedById = adminId,
        };
        db.TermsOfServices.Add(terms);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Đã xuất bản điều khoản phiên bản {version} thành công. Người dùng sẽ được yêu cầu đồng ý ở lần đăng nhập tiếp theo.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /TermsPolicy/Activate/{id} — Kích hoạt lại phiên bản cũ
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var terms = await db.TermsOfServices.FindAsync(id);
        if (terms == null) return NotFound();

        await db.TermsOfServices
            .Where(t => t.IsActive)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));

        terms.IsActive = true;
        await db.SaveChangesAsync();

        TempData["Success"] = $"Đã kích hoạt lại phiên bản {terms.Version}.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /TermsPolicy/Preview/{id}
    public async Task<IActionResult> Preview(int id)
    {
        var terms = await db.TermsOfServices.FindAsync(id);
        if (terms == null) return NotFound();
        return View(terms);
    }

    // GET: /TermsPolicy/AgreementStats/{id}
    public async Task<IActionResult> AgreementStats(int id)
    {
        var terms = await db.TermsOfServices
            .Include(t => t.Agreements).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (terms == null) return NotFound();
        return View(terms);
    }
}
