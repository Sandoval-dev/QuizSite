using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizSite.Services;
using QuizSite.Data;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddTransient<IEmailService, SmtpEmailService>();
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Quiz360.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<DataContext>(options =>
{
    string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // 1. Veritabanı migrate edilsin
    var context = services.GetRequiredService<DataContext>();
    context.Database.Migrate(); // 👈 Bu satır en önemli adım

    // 2. Admin bilgileri
    var config = services.GetRequiredService<IConfiguration>();
    var adminEmail = config["AdminUser:Email"];
    var adminPassword = config["AdminUser:Password"];

    // 3. Identity seed işlemi
    await IdentityInitializer.SeedAsync(services, adminEmail!, adminPassword!);
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
