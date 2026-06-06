using DnsClient;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.RegularExpressions;

namespace CarWebsite.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger)
{
    // Trả về null nếu hợp lệ, trả về chuỗi lỗi nếu không hợp lệ
    public async Task<string?> ValidateEmailAsync(string email)
    {
        email = email.Trim().ToLower();

        // 1. Format cơ bản
        if (!email.Contains('@') || !email.Contains('.'))
            return "Email không đúng định dạng.";

        var parts = email.Split('@');
        if (parts.Length != 2) return "Email không đúng định dạng.";

        var username = parts[0];
        var domain   = parts[1];

        // 2. Quy tắc riêng cho Gmail
        if (domain == "gmail.com")
        {
            var clean = username.Replace(".", "");
            if (clean.Length < 6 || clean.Length > 30)
                return "Địa chỉ Gmail phải có từ 6 đến 30 ký tự (không tính dấu chấm).";
            if (!Regex.IsMatch(username, @"^[a-z0-9]+(\.[a-z0-9]+)*$"))
                return "Gmail chỉ được chứa chữ thường, số và dấu chấm, không có ký tự đặc biệt.";
            if (username.StartsWith('.') || username.EndsWith('.') || username.Contains(".."))
                return "Gmail không được bắt đầu, kết thúc hoặc có hai dấu chấm liên tiếp.";
        }

        // 3. Kiểm tra MX record — domain có mail server không?
        try
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(domain, QueryType.MX);
            if (!result.Answers.MxRecords().Any())
                return $"Tên miền '{domain}' không có máy chủ email. Vui lòng kiểm tra lại địa chỉ email.";
        }
        catch
        {
            return $"Không thể xác minh tên miền '{domain}'. Vui lòng kiểm tra lại địa chỉ email.";
        }

        return null; // hợp lệ
    }

    private MimeMessage BuildMessage(string toEmail, string toName, string subject, string htmlBody)
    {
        var from    = config["Email:FromEmail"]!;
        var msg     = new MimeMessage();
        msg.From.Add(new MailboxAddress("AutoHT", from));
        msg.To.Add(new MailboxAddress(toName, toEmail));
        msg.Subject = subject;
        msg.Body    = new TextPart("html") { Text = htmlBody };
        return msg;
    }

    private async Task SendAsync(MimeMessage msg)
    {
        var from    = config["Email:FromEmail"]!;
        var appPass = config["Email:AppPassword"]!;
        var host    = config["Email:SmtpHost"] ?? "smtp.gmail.com";
        var port    = int.Parse(config["Email:SmtpPort"] ?? "587");

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(from, appPass);
        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
    }

    public async Task SendVerificationCodeAsync(string toEmail, string toName, string code)
    {
        var digits = string.Join(
            "</td><td style='width:48px;height:56px;text-align:center;font-size:28px;font-weight:700;color:#001C3D;background:#f3f4f6;border-radius:8px;border:2px solid #e5e7eb;padding:0'>",
            code.ToCharArray());

        var body = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <div style="background:#001C3D;padding:24px;border-radius:12px 12px 0 0">
                <h1 style="color:#fff;font-size:22px;margin:0">Auto<span style="color:#D71920">HT</span></h1>
              </div>
              <div style="background:#f9f9f9;padding:32px;border-radius:0 0 12px 12px;border:1px solid #e5e7eb">
                <h2 style="color:#001C3D;margin-top:0">Mã xác minh tài khoản</h2>
                <p style="color:#4b5563">Xin chào <strong>{toName}</strong>,</p>
                <p style="color:#4b5563">Nhập mã 6 chữ số bên dưới để kích hoạt tài khoản:</p>
                <table style="border-collapse:separate;border-spacing:8px;margin:24px auto">
                  <tr>
                    <td style="width:48px;height:56px;text-align:center;font-size:28px;font-weight:700;color:#001C3D;background:#f3f4f6;border-radius:8px;border:2px solid #e5e7eb;padding:0">{digits}</td>
                  </tr>
                </table>
                <p style="color:#9ca3af;font-size:13px;text-align:center">
                  Mã hết hạn sau <strong>15 phút</strong>. Nếu bạn không đăng ký, hãy bỏ qua email này.
                </p>
                <hr style="border:none;border-top:1px solid #e5e7eb;margin:20px 0"/>
                <p style="color:#9ca3af;font-size:12px">AutoHT — 123 Đường Lê Lợi, Q.1, TP.HCM</p>
              </div>
            </div>
            """;

        var msg = BuildMessage(toEmail, toName, $"Mã xác minh AutoHT: {code}", body);
        await SendAsync(msg);
        logger.LogInformation("Verification code sent to {Email}", toEmail);
    }

    public async Task SendVerificationEmailAsync(string toEmail, string toName, string verifyUrl)
    {
        var body = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto">
              <div style="background:#001C3D;padding:24px;border-radius:12px 12px 0 0">
                <h1 style="color:#fff;font-size:22px;margin:0">Auto<span style="color:#D71920">HT</span></h1>
              </div>
              <div style="background:#f9f9f9;padding:32px;border-radius:0 0 12px 12px;border:1px solid #e5e7eb">
                <h2 style="color:#001C3D;margin-top:0">Xác minh địa chỉ email</h2>
                <p style="color:#4b5563">Xin chào <strong>{toName}</strong>,</p>
                <p style="color:#4b5563">Nhấn nút bên dưới để xác minh email và kích hoạt tài khoản.</p>
                <a href="{verifyUrl}" style="display:inline-block;background:#D71920;color:#fff;font-weight:700;padding:12px 28px;border-radius:10px;text-decoration:none;margin:16px 0">
                  Xác minh Email
                </a>
                <p style="color:#9ca3af;font-size:13px">Link hết hạn sau <strong>24 giờ</strong>.</p>
                <hr style="border:none;border-top:1px solid #e5e7eb;margin:20px 0"/>
                <p style="color:#9ca3af;font-size:12px">AutoHT — 123 Đường Lê Lợi, Q.1, TP.HCM</p>
              </div>
            </div>
            """;

        var msg = BuildMessage(toEmail, toName, "Xác minh tài khoản AutoHT", body);
        await SendAsync(msg);
    }
}
