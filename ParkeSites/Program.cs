using BusiniessLayer.Abstract;
using BusiniessLayer.Concrete;
using DataAcsessLayer.Abstract;
using DataAcsessLayer.Concrete.Context;
using DataAcsessLayer.EntityFramework;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// --- EKSÝK OLAN DI KAYDI EKLENDÝ ---
// UserManager içindeki _httpContextAccessor için bu þart!
builder.Services.AddHttpContextAccessor();

// Dependency Injection
builder.Services.AddScoped<IProjectDal, EFProjectDal>();
//builder.Services.AddScoped<IProjectService, ProjectManager>(); // Ýleride açarsýn

builder.Services.AddScoped<IProjectImageDal, EFProjectImageDal>();
//builder.Services.AddScoped<IProjectImageService, ProjectImageManager>(); // Ýleride açarsýn

builder.Services.AddScoped<IUserDal, EFUserDal>();
builder.Services.AddScoped<IUserService, UserManager>();

// Authentication (Cookie Ayarlarý Harika!)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisi olmayan biri girerse buraya atar (Opsiyonel ama iyidir)
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.Cookie.Name = "ArslanParkeAdminAuth";
        options.SlidingExpiration = true; // Kullanýcý aktifse 7 günlük süreyi sürekli uzatýr
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
app.UseRouting();

// Authentication and Authorization sýralamasý çok doðru
app.UseAuthentication();
app.UseAuthorization();

//app.MapStaticAssets();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    

app.Run();