using System.Security.Claims;
using CarWebsite.Data;
using CarWebsite.Models;
using CarWebsite.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWebsite.Controllers;

public class AccountController(AppDbContext db, EmailService emailSvc, ILogger<AccountController> logger) : Controller
{
    public IActionResult Login(string? returnUrl)
    {
        if (HttpContext.Session.GetString("UserId") != null)
            return RedirectToAction("Index", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View();
        }
        if (!user.IsActive)
        {
            ModelState.AddModelError("", "Tài khoản này đã bị khóa. Vui lòng liên hệ quản trị viên.");
            return View();
        }

        if (user.Role == "customer" && !user.EmailVerified)
        {
            TempData["PendingEmail"] = email;
            return RedirectToAction("VerifyCode");
        }

        // Lưu thông tin vào Session (tương thích cũ)
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserAvatar", user.Avatar ?? "");

        // Phát Claims Cookie để dùng [Authorize(Roles="...")]
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,  user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role,  user.Role),
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        // Kiểm tra xem người dùng đã đồng ý phiên bản điều khoản mới nhất chưa
        var activeTerms = await db.TermsOfServices
            .Where(t => t.IsActive)
            .FirstOrDefaultAsync();
        if (activeTerms != null)
        {
            bool hasAgreed = await db.UserTermAgreements
                .AnyAsync(a => a.UserId == user.Id && a.TermsOfServiceId == activeTerms.Id);
            if (!hasAgreed)
            {
                HttpContext.Session.SetString("PendingTermsId", activeTerms.Id.ToString());
                HttpContext.Session.SetString("PendingTermsReturnUrl", returnUrl ?? "");
                return RedirectToAction(nameof(AcceptNewTerms));
            }
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return Roles.AllStaff.Contains(user.Role)
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> AcceptNewTerms()
    {
        var termsIdStr = HttpContext.Session.GetString("PendingTermsId");
        if (string.IsNullOrEmpty(termsIdStr) || !int.TryParse(termsIdStr, out var termsId))
            return RedirectToAction("Index", "Home");

        var terms = await db.TermsOfServices.FindAsync(termsId);
        if (terms == null) return RedirectToAction("Index", "Home");

        return View(terms);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptNewTerms(int termsId)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login");

        var terms = await db.TermsOfServices.FindAsync(termsId);
        if (terms == null) return RedirectToAction("Login");

        bool alreadyAgreed = await db.UserTermAgreements
            .AnyAsync(a => a.UserId == userId && a.TermsOfServiceId == termsId);

        if (!alreadyAgreed)
        {
            db.UserTermAgreements.Add(new UserTermAgreement
            {
                UserId           = userId,
                TermsOfServiceId = termsId,
                AgreedAt         = DateTime.UtcNow,
                IpAddress        = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            await db.SaveChangesAsync();
        }

        var returnUrl = HttpContext.Session.GetString("PendingTermsReturnUrl") ?? "";
        HttpContext.Session.Remove("PendingTermsId");
        HttpContext.Session.Remove("PendingTermsReturnUrl");

        var role = HttpContext.Session.GetString("UserRole") ?? "";
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return Roles.AllStaff.Contains(role)
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Register()
    {
        ViewBag.ActiveTerms = await db.TermsOfServices.Where(t => t.IsActive).FirstOrDefaultAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword, bool agreeTerms = false)
    {
        if (!agreeTerms)
        {
            ModelState.AddModelError("", "Bạn phải đồng ý với Điều khoản sử dụng để đăng ký.");
            return View();
        }

        if (password != confirmPassword)
        {
            ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
            return View();
        }

        var emailError = await emailSvc.ValidateEmailAsync(email);
        if (emailError != null)
        {
            ModelState.AddModelError("", emailError);
            return View();
        }

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError("", "Email này đã được sử dụng.");
            return View();
        }

        var code = Random.Shared.Next(100000, 1000000).ToString();
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "customer",
            EmailVerified = false,
            VerificationToken = code,
            VerificationExpiry = DateTime.UtcNow.AddMinutes(15)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Lưu vết đồng ý điều khoản khi đăng ký
        var activeTerms = await db.TermsOfServices.Where(t => t.IsActive).FirstOrDefaultAsync();
        if (activeTerms != null)
        {
            db.UserTermAgreements.Add(new UserTermAgreement
            {
                UserId           = user.Id,
                TermsOfServiceId = activeTerms.Id,
                AgreedAt         = DateTime.UtcNow,
                IpAddress        = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            await db.SaveChangesAsync();
        }

        try
        {
            await emailSvc.SendVerificationCodeAsync(user.Email, user.Name, code);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gửi email xác minh thất bại tới {Email}", email);
            TempData["EmailError"] = "Không thể gửi email xác minh. Vui lòng dùng nút \"Gửi lại mã\" ở bước tiếp theo.";
        }

        TempData["PendingEmail"] = email;
        return RedirectToAction("VerifyCode");
    }

    [HttpGet]
    public IActionResult VerifyCode()
    {
        var email = TempData["PendingEmail"] as string;
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Register");

        ViewBag.Email = email;
        ViewBag.ResendSuccess = TempData["ResendSuccess"];
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCode(string email, string code)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && !u.EmailVerified);

        if (user == null)
        {
            ViewBag.Email = email;
            ModelState.AddModelError("", "Không tìm thấy tài khoản chờ xác minh.");
            return View();
        }

        if (user.VerificationExpiry < DateTime.UtcNow)
        {
            ViewBag.Email = email;
            ModelState.AddModelError("", "Mã đã hết hạn. Vui lòng nhấn \"Gửi lại mã\".");
            return View();
        }

        if (user.VerificationToken != code.Trim())
        {
            ViewBag.Email = email;
            ModelState.AddModelError("", "Mã xác minh không đúng. Vui lòng kiểm tra lại.");
            return View();
        }

        user.EmailVerified = true;
        user.VerificationToken = null;
        user.VerificationExpiry = null;
        await db.SaveChangesAsync();

        TempData["Success"] = "Xác minh email thành công! Bạn có thể đăng nhập ngay bây giờ.";
        return RedirectToAction("Login");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendCode(string email)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && !u.EmailVerified);

        if (user != null)
        {
            var code = Random.Shared.Next(100000, 1000000).ToString();
            user.VerificationToken = code;
            user.VerificationExpiry = DateTime.UtcNow.AddMinutes(15);
            await db.SaveChangesAsync();
            try
            {
                await emailSvc.SendVerificationCodeAsync(user.Email, user.Name, code);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Gửi lại email thất bại tới {Email}", email);
            }

        }

        TempData["PendingEmail"] = email;
        TempData["ResendSuccess"] = "Mã xác minh mới đã được gửi đến email của bạn.";
        return RedirectToAction("VerifyCode");
    }

    public async Task<IActionResult> VerifyEmail(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction("Login");

        var user = await db.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

        if (user == null)
        {
            TempData["Error"] = "Link xác minh không hợp lệ.";
            return RedirectToAction("Login");
        }

        if (user.VerificationExpiry < DateTime.UtcNow)
        {
            TempData["Error"] = "Link xác minh đã hết hạn. Vui lòng liên hệ admin để gửi lại.";
            return RedirectToAction("Login");
        }

        user.EmailVerified = true;
        user.VerificationToken = null;
        user.VerificationExpiry = null;
        await db.SaveChangesAsync();

        TempData["Success"] = "Xác minh email thành công! Bạn có thể đăng nhập ngay bây giờ.";
        return RedirectToAction("Login");
    }

    public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        TempData["Success"] = "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu.";
        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // ── Đăng nhập bằng Google / Facebook ────────────────────────────────────

    [HttpGet]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalCallback), "Account", new { returnUrl });
        var props = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(props, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalCallback(string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync("ExternalCookie");
        if (!result.Succeeded)
        {
            TempData["Error"] = "Đăng nhập bên ngoài thất bại. Vui lòng thử lại.";
            return RedirectToAction("Login");
        }

        var email  = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;
        var name   = result.Principal?.FindFirst(ClaimTypes.Name)?.Value;
        var avatar = result.Principal?.FindFirst("urn:google:picture")?.Value
                  ?? result.Principal?.FindFirst("picture")?.Value;

        await HttpContext.SignOutAsync("ExternalCookie");

        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Không lấy được email từ tài khoản. Vui lòng thử lại hoặc dùng email/mật khẩu.";
            return RedirectToAction("Login");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            user = new User
            {
                Email         = email,
                Name          = name ?? email.Split('@')[0],
                Role          = "customer",
                EmailVerified = true,
                IsActive      = true,
                Avatar        = avatar,
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var activeTerms = await db.TermsOfServices.Where(t => t.IsActive).FirstOrDefaultAsync();
            if (activeTerms != null)
            {
                db.UserTermAgreements.Add(new UserTermAgreement
                {
                    UserId           = user.Id,
                    TermsOfServiceId = activeTerms.Id,
                    AgreedAt         = DateTime.UtcNow,
                    IpAddress        = HttpContext.Connection.RemoteIpAddress?.ToString(),
                });
                await db.SaveChangesAsync();
            }
        }
        else if (!user.IsActive)
        {
            TempData["Error"] = "Tài khoản này đã bị khóa. Vui lòng liên hệ quản trị viên.";
            return RedirectToAction("Login");
        }

        if (string.IsNullOrEmpty(user.Avatar) && !string.IsNullOrEmpty(avatar))
        {
            user.Avatar = avatar; await db.SaveChangesAsync();
        }

        HttpContext.Session.SetString("UserId",    user.Id.ToString());
        HttpContext.Session.SetString("UserName",  user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole",  user.Role);
        HttpContext.Session.SetString("UserAvatar", user.Avatar ?? "");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,  user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role,  user.Role),
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return Roles.AllStaff.Contains(user.Role)
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Index", "Home");
    }
}
