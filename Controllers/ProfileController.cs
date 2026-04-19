using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;

namespace Apex7.Controllers
{
    [Authorize] // Доступ только для авторизованных
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Страница профиля
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity!.Name;
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null) return RedirectToAction("Login", "Account");

            return View(user);
        }

        // Сохранение изменений
        [HttpPost]
        public async Task<IActionResult> Edit(string fullName, string phone)
        {
            var userEmail = User.Identity!.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null)
            {
                user.FullName = fullName;
                user.Phone = phone;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Данные успешно обновлены";
            }

            return RedirectToAction("Index");
        }
    }
}