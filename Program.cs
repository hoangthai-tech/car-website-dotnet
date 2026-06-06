using CarWebsite.Data;
using CarWebsite.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 500_000_000; // 500 MB
});

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Xác thực bằng Cookie + Claims (dùng song song với Session)
var authBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath  = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly   = true;
        options.Cookie.IsEssential = true;
    })
    .AddCookie("ExternalCookie", o =>
    {
        o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        o.Cookie.SameSite    = SameSiteMode.Unspecified;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Google OAuth — chỉ bật khi đã điền ClientId vào appsettings.json
var googleId = builder.Configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrEmpty(googleId))
    authBuilder.AddGoogle(o =>
    {
        o.SignInScheme = "ExternalCookie";
        o.ClientId     = googleId;
        o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });

// Facebook OAuth — chỉ bật khi đã điền AppId vào appsettings.json
var fbId = builder.Configuration["Authentication:Facebook:AppId"];
if (!string.IsNullOrEmpty(fbId))
    authBuilder.AddFacebook(o =>
    {
        o.SignInScheme = "ExternalCookie";
        o.AppId       = fbId;
        o.AppSecret   = builder.Configuration["Authentication:Facebook:AppSecret"]!;
        o.CorrelationCookie.SameSite    = SameSiteMode.Unspecified;
        o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
var mimeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
mimeProvider.Mappings[".glb"] = "model/gltf-binary";
mimeProvider.Mappings[".gltf"] = "model/gltf+json";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = mimeProvider });
app.UseRouting();
app.UseSession();
app.UseAuthentication();   // phải trước UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "xe-datmua",
    pattern: "Xe/DatMua",
    defaults: new { controller = "Xe", action = "DatMua" });

app.MapControllerRoute(
    name: "xe-suggest",
    pattern: "Xe/SearchSuggest",
    defaults: new { controller = "Xe", action = "SearchSuggest" });

app.MapControllerRoute(
    name: "xe-viewer3d",
    pattern: "xe/{slug}/3d",
    defaults: new { controller = "Xe", action = "Viewer3D" });

app.MapControllerRoute(
    name: "xe-detail",
    pattern: "xe/{slug}",
    defaults: new { controller = "Xe", action = "Detail" });

app.MapControllerRoute(
    name: "tintuc-detail",
    pattern: "tin-tuc/{slug}",
    defaults: new { controller = "TinTuc", action = "Detail" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

app.Run();
