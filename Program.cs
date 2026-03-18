using Microsoft.EntityFrameworkCore;
using T1EsportsWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(); // <--- THÊM DÒNG NÀY ĐỂ BẬT SESSION



// Cấu hình kết nối Database bằng T1DbContext
builder.Services.AddDbContext<T1DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Cookie Authentication (Đăng nhập)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    })
    .AddGoogle(options =>
    {
        // Bạn cần tạo dự án trên Google Cloud Console để lấy 2 mã này nhé
        options.ClientId = "66135686344-2uk3ot494r7pivmt1grk0d7s1f0i7mr1.apps.googleusercontent.com";
        options.ClientSecret = "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";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// Chú ý: Phải Authentication (Xác thực) trước rồi mới Authorization (Phân quyền)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();