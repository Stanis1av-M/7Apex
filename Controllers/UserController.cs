using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;
using Apex7.Models; // ДОБАВЛЕНО: Для доступа к EditProfileViewModel
using System.Security.Claims;

namespace Apex7.Controllers
{
    [Authorize] // Доступ только для вошедших пользователей
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ЛИЧНЫЙ КАБИНЕТ (Профиль)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.Role) // Обязательно подгружаем роль для отображения
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            return View(user);
        }

        // ==========================================
        // 1.5. РЕДАКТИРОВАНИЕ ПРОФИЛЯ (НОВОЕ)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            // Передаем текущие данные в форму
            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null) return NotFound();

                // Проверяем, не занят ли новый Email кем-то другим
                if (user.Email != model.Email)
                {
                    var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "Этот Email уже занят другим пользователем");
                        return View(model);
                    }
                }

                // Обновляем данные
                user.FullName = model.FullName;
                user.Email = model.Email;

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Профиль успешно обновлен! (Чтобы имя обновилось в меню, перезайдите в систему)";
                return RedirectToAction(nameof(Index));
            }

            // Если есть ошибки валидации, возвращаем форму обратно
            return View(model);
        }

        // ==========================================
        // 2. ОБРАТНАЯ СВЯЗЬ
        // ==========================================
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFeedback(Feedback model)
        {
            // 1. Проверяем, залогинен ли пользователь
            var userId = GetCurrentUserId();

            // 2. Если модель заполнена корректно (текст сообщения есть)
            if (ModelState.IsValid)
            {
                model.UserId = userId; // Привязываем ID пользователя из системы
                model.CreatedAt = DateTime.Now;
                model.Status = FeedbackStatus.Pending; // Устанавливаем статус "В ожидании"

                _context.Feedbacks.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Ваше обращение принято! Мы ответим вам в ближайшее время.";
                return RedirectToAction(nameof(MyRequests));
            }

            // Если что-то не так, возвращаем на страницу с ошибками
            return View("Contact", model);
        }

        // ==========================================
        // 3. МОИ ЗАЯВКИ (Feedbacks)
        // ==========================================
        public async Task<IActionResult> MyRequests()
        {
            var userEmail = User.Identity?.Name;
            var requests = await _context.Feedbacks
                .Where(f => f.Email == userEmail) // В твоей БД связь идет по Email
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // ==========================================
        // 4. ИСТОРИЯ ЗАКАЗОВ
        // ==========================================
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetCurrentUserId();

            var orders = await _context.Orders
                .Include(o => o.OrderStatus) // Чтобы знать "Новый" или "Оплачен"
                .Include(o => o.OrderItems)   // Чтобы знать список позиций
                    .ThenInclude(oi => oi.Product) // ВАЖНО: Без этого названия товаров не отобразятся!
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // Вспомогательный метод для получения ID текущего юзера
        private int GetCurrentUserId()
        {
            // Ищем Claim с ID. Важно, чтобы при логине ты его записывал.
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (claim != null && int.TryParse(claim.Value, out int id))
            {
                return id;
            }

            // Если по ID не нашли, ищем по Email (как запасной вариант)
            var email = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            return user?.UserId ?? 0;
        }
    }
}