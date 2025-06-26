using System.Text;
using BlogShare.Web.Data;
using BlogShare.Web.Hubs;
using BlogShare.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

// Thêm dịch vụ DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddSignalR();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Users.Any(u => u.Role == "Admin"))
    {
        db.Users.Add(new User
        {
            FullName = "Quản trị viên",
            Email = "admin@blog.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // ✅ Sửa chỗ này
            Role = "Admin"
        });
        db.SaveChanges();   
    }
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new Category { Name = "Tin tức" },
            new Category { Name = "Thủ thuật" },
            new Category { Name = "Chia sẻ cá nhân" }
        );
        db.SaveChanges();
    }
}
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});
app.UseSession();
app.UseRouting();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.MapHub<ChatHub>("/chathub");
app.Run();
