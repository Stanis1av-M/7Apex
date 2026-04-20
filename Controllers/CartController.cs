using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apex7.Controllers
{
    [Authorize] // Только авторизованные
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context) => _context = context;

        // Показать корзину
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity!.Name;
            var user = await _context.Users
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            return View(user?.CartItems.ToList() ?? new List<CartItem>());
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

        // --- МЕТОД ОФОРМЛЕНИЯ ЗАКАЗА ---
        
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userEmail = User.Identity!.Name;
            var user = await _context.Users
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null || !user.CartItems.Any()) return RedirectToAction("Index");

            // 1. НАХОДИМ ID В БАЗЕ (Чтобы не хардкодить 1, 2, 3...)
            var status = await _context.OrderStatuses.FirstOrDefaultAsync();
            var delivery = await _context.DeliveryMethods.FirstOrDefaultAsync();
            var payment = await _context.PaymentMethods.FirstOrDefaultAsync();

            // Если в таблицах вообще пусто, вернем ошибку
            if (status == null || delivery == null || payment == null)
            {
                TempData["Error"] = "Ошибка системы: не найдены справочники статусов или методов доставки.";
                return RedirectToAction("Index");
            }

            // 2. Создаем Заказ
            var order = new Order
            {
                UserId = user.UserId,
                OrderStatusId = status.OrderStatusID,
                DeliveryMethodId = delivery.DeliveryMethodId, // Проверь здесь имя свойства!
                PaymentMethodId = payment.PaymentMethodId,
                CreatedAt = DateTime.Now,
                TotalAmount = user.CartItems.Sum(ci => ci.Product.Price * ci.Quantity)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 3. Создаем позиции заказа
            foreach (var item in user.CartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });
            }

            // 4. Очищаем корзину
            _context.CartItems.RemoveRange(user.CartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Заказ успешно оформлен!";
            return RedirectToAction("Index", "Home");
        }
    }
}