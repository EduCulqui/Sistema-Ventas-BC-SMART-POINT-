using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Sistema_BC_SMART_POINT.Data;
using Sistema_BC_SMART_POINT.Services;

var builder = WebApplication.CreateBuilder(args);


// Session (para el carrito)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Autenticación 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Auth/Login";
        opt.AccessDeniedPath = "/Auth/Login";
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddScoped<AuthService>();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("StringSQL"));
});

// Servicios propios (inyección de dependencias)
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CarritoService>();
builder.Services.AddScoped<VentaService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();        
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");  // La home es el catálogo

app.Run();

