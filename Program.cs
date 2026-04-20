using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Microsoft.AspNetCore.Authentication.Cookies; // Нужно для Cookies

var builder = WebApplication.CreateBuilder(args);

// 1. ПОДКЛЮЧАЕМ СТРОКУ СОЕДИНЕНИЯ
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. РЕГИСТРИРУЕМ КОНТЕКСТ БАЗЫ ДАННЫХ
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. НАСТРАИВАЕМ АУТЕНТИФИКАЦИЮ (Система входа)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Куда отправлять гостя при попытке зайти в профиль
        options.AccessDeniedPath = "/Account/AccessDenied"; // Если нет прав (например, клиент лезет в админку)
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ============================================================
// БЛОК ИНИЦИАЛИЗАЦИИ БАЗЫ ДАННЫХ (Seed Data)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        throw; 
    }
}
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();  
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();