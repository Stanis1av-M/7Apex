using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;

namespace Apex7.Controllers
{
    [Authorize(Roles = "Администратор, Менеджер")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();

            // Список всех ролей для выпадающего списка
            ViewBag.RolesList = await _context.Roles.ToListAsync();

            return View(users);
        }

        // POST: /Admin/UpdateRole
        [HttpPost]
        [Authorize(Roles = "Администратор")] // меняет роль только админ
        public async Task<IActionResult> UpdateRole(int userId, int newRoleId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == newRoleId);
            if (!roleExists)
            {
                TempData["Error"] = "Выбранная роль не существует";
                return RedirectToAction(nameof(Users));
            }

            user.RoleId = newRoleId;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Роль пользователя {user.Email} изменена";
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [Authorize(Roles = "Администратор")] // удаляет только админ
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Не даём удалить самого себя (по email текущего пользователя)
            var currentUserEmail = User.Identity.Name;
            if (user.Email == currentUserEmail)
            {
                TempData["Error"] = "Нельзя удалить свою собственную учётную запись";
                return RedirectToAction(nameof(Users));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Пользователь {user.Email} удалён";
            return RedirectToAction(nameof(Users));
        }
    }
}