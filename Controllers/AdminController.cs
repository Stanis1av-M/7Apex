using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;
using System.Security.Claims;

namespace Apex7.Controllers
{
    [Authorize(Roles = "Администратор, Менеджер")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index() => View();

        // ==========================================
        // 1. УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ
        // ==========================================

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name != "Администратор") // Скрываем других админов
                .AsNoTracking()
                .ToListAsync();

            ViewBag.RolesList = await _context.Roles.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int userId, int newRoleId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId.ToString() == currentUserId)
            {
                TempData["Error"] = "Вы не можете изменить роль собственному аккаунту.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.RoleId = newRoleId;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Права доступа для {user.FullName} изменены.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBan(int userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId.ToString() == currentUserId)
            {
                TempData["Error"] = "Запрещено блокировать собственный аккаунт.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Ban = !user.Ban;
            await _context.SaveChangesAsync();
            TempData["Success"] = user.Ban ? "Доступ пользователю закрыт." : "Доступ восстановлен.";
            return RedirectToAction(nameof(Users));
        }

        // --- ДОБАВЛЕННЫЙ МЕТОД: УДАЛЕНИЕ ПОЛЬЗОВАТЕЛЯ (Исправляет 404) ---
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id) // Параметр должен называться id
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Защита: нельзя удалить самого себя
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id.ToString() == currentUserId)
            {
                TempData["Error"] = "Удаление собственного аккаунта невозможно.";
                return RedirectToAction(nameof(Users));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Пользователь {user.FullName} полностью удален из базы.";
            return RedirectToAction(nameof(Users));
        }

        // ==========================================
        // 2. УПРАВЛЕНИЕ ЗАКАЗАМИ
        // ==========================================

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Statuses = await _context.OrderStatuses.ToListAsync();
            return View("~/Views/Manager/Orders.cshtml", orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, int newStatusId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.OrderStatusId = newStatusId;
            order.CompletedAt = (newStatusId == 5) ? DateTime.Now : null;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Статус заказа обновлен.";
            return RedirectToAction(nameof(Orders));
        }

        // ==========================================
        // 3. УПРАВЛЕНИЕ НОВОСТЯМИ
        // ==========================================

        public async Task<IActionResult> NewsList()
        {
            var news = await _context.News.OrderByDescending(n => n.CreatedAt).ToListAsync();
            return View(news);
        }

        [HttpGet]
        [Authorize(Roles = "Администратор")]
        public IActionResult CreateNews() => View();

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNews(News model, IFormFile? image1, IFormFile? image2)
        {
            if (ModelState.IsValid)
            {
                model.ImageURL = await SaveFile(image1);
                model.ImageURL2 = await SaveFile(image2);
                model.CreatedAt = DateTime.Now;
                _context.News.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(NewsList));
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> EditNews(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();
            return View(news);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNews(News model, IFormFile? image1, IFormFile? image2)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.News.AsNoTracking().FirstOrDefaultAsync(n => n.NewsID == model.NewsID);
                if (existing == null) return NotFound();

                model.ImageURL = image1 != null ? await SaveFile(image1) : existing.ImageURL;
                model.ImageURL2 = image2 != null ? await SaveFile(image2) : existing.ImageURL2;
                model.CreatedAt = existing.CreatedAt;
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(NewsList));
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news); // Полное удаление
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(NewsList));
        }

        // ==========================================
        // 4. ОБРАТНАЯ СВЯЗЬ
        // ==========================================

        public async Task<IActionResult> Feedbacks()
        {
            var list = await _context.Feedbacks
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewFeedback(int id, FeedbackStatus status, string? adminResponse)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return NotFound();

            feedback.Status = status;
            feedback.AdminResponse = adminResponse;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Решение по обращению сохранено.";
            return RedirectToAction(nameof(Feedbacks));
        }

        private async Task<string?> SaveFile(IFormFile? file)
        {
            if (file == null) return null;
            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/news");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return "/images/news/" + uniqueFileName;
        }
    }
}