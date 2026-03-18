using BusiniessLayer.Abstract;
using BusiniessLayer.Concrete;
using DataAcsessLayer.Abstract;
using DataAcsessLayer.Concrete.Context;
using DataAcsessLayer.EntityFramework;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;

// YENİ: Serilog Ayarları (Gereksiz Loglar Filtrelendi)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    // ŞU 3 SATIR İLE SİSTEM VE SQL LOGLARINI SUSTURUYORUZ (Sadece Hata/Uyarı varsa yazacak)
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/arslanparke-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

    builder.Services.AddHttpContextAccessor();

    // Dependency Injection
    builder.Services.AddScoped<IProjectDal, EFProjectDal>();
    builder.Services.AddScoped<ProjectService, ProjectManager>();

    builder.Services.AddScoped<IProjectImageDal, EFProjectImageDal>();
    builder.Services.AddScoped<IUserDal, EFUserDal>();
    builder.Services.AddScoped<IUserService, UserManager>();

    // Authentication 
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";

            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = false;

            options.Cookie.Name = "ArslanParkeAdminAuth";
        });

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseStaticFiles();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama beklenmedik bir şekilde çöktü!");
}
finally
{
    Log.CloseAndFlush();
}