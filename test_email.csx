using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var from    = config["Email:FromEmail"]!;
var appPass = config["Email:AppPassword"]!;
var host    = config["Email:SmtpHost"] ?? "smtp.gmail.com";
var port    = int.Parse(config["Email:SmtpPort"] ?? "587");

Console.WriteLine($"From: {from}");
Console.WriteLine($"Host: {host}:{port}");
Console.WriteLine($"Pass len: {appPass.Length}");

try {
    var msg = new MimeMessage();
    msg.From.Add(new MailboxAddress("AutoHT", from));
    msg.To.Add(new MailboxAddress("Test", from));
    msg.Subject = "Test MailKit";
    msg.Body = new TextPart("plain") { Text = "Test email from MailKit" };

    using var client = new SmtpClient();
    Console.WriteLine("Connecting...");
    await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
    Console.WriteLine("Connected. Authenticating...");
    await client.AuthenticateAsync(from, appPass);
    Console.WriteLine("Authenticated. Sending...");
    await client.SendAsync(msg);
    await client.DisconnectAsync(true);
    Console.WriteLine("SUCCESS!");
} catch (Exception ex) {
    Console.WriteLine($"ERROR: {ex.GetType().Name}");
    Console.WriteLine($"Message: {ex.Message}");
    if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
}
