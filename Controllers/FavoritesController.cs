using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;

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
            var userEmail = User.Identity!.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            var favorites = await _context.FavoriteProducts
                .Include(f => f.Product)
                .ThenInclude(p => p.Manufacturer) // Чтобы выводить бренд
                .Where(f => f.UserId == user!.UserId)
                .ToListAsync();

            return View(favorites);
        }

        // Добавить / Удалить из избранного (Toggle)
        [HttpPost]
        public async Task<IActionResult> Toggle(int productId, string returnUrl)
        {
            var userEmail = User.Identity!.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            var existingFavorite = await _context.FavoriteProducts
                .FirstOrDefaultAsync(f => f.UserId == user!.UserId && f.ProductId == productId);

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
                    UserId = user!.UserId,
                    ProductId = productId,
                    AddedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            // Возвращаем пользователя туда, откуда он нажал кнопку
            return Redirect(returnUrl);
        }
    }
}