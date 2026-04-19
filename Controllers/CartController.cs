using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;
using System.Security.Claims;

namespace Apex7.Controllers
{
    [Authorize] // В корзину могут заходить только авторизованные
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Показать корзину
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity!.Name;
            var user = await _context.Users
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            return View(user?.CartItems ?? new List<CartItem>());
        }

        // Добавить товар
        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var userEmail = User.Identity!.Name;
            var user = await _context.Users
                .Include(u => u.CartItems)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null) return RedirectToAction("Login", "Account");

            // Проверяем, есть ли такой товар уже в корзине
            var existingItem = user.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = user.UserId,
                    ProductId = productId,
                    Quantity = 1
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index"); 
        }

        // Удалить товар
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}