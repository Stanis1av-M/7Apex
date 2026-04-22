using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;
using System.Security.Claims;

namespace Apex7.Controllers
{
    [Authorize] // Только для залогиненных
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FavoritesController(ApplicationDbContext context) => _context = context;

        // Открыть страницу Избранного
        public async Task<IActionResult> Index()
        {
            // 1. Берем ID сразу из "паспорта" (куки), без лишних запросов в базу!
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Если ID нет или он кривой — выкидываем на страницу входа
            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Ищем товары, используя уже готовую переменную userId
            var favorites = await _context.FavoriteProducts
                .Include(f => f.Product)
                .ThenInclude(p => p.Manufacturer) // Выводим бренд
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return View(favorites);
        }

        // Добавить / Удалить из избранного (Toggle)
        [HttpPost]
        public async Task<IActionResult> Toggle(int productId, string returnUrl)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var existingFavorite = await _context.FavoriteProducts
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existingFavorite != null)
            {
                // Если уже есть — удаляем
                _context.FavoriteProducts.Remove(existingFavorite);
            }
            else
            {
                // Если нет — добавляем
                _context.FavoriteProducts.Add(new FavoriteProduct
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            // Возвращаем пользователя туда, откуда он нажал кнопку
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index"); // Запасной вариант, если returnUrl пустой
        }
    }
}