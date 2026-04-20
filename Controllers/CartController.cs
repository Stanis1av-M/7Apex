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
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity!.Name;
            var user = await _context.Users
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            return View(user?.CartItems.ToList() ?? new List<CartItem>());
        }

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

        // --- ОБНОВЛЕННЫЙ МЕТОД ОФОРМЛЕНИЯ ЗАКАЗА СО СПИСАНИЕМ ---

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userEmail = User.Identity!.Name;
            var user = await _context.Users
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null || !user.CartItems.Any()) return RedirectToAction("Index");

            // Находим справочные данные
            var status = await _context.OrderStatuses.FirstOrDefaultAsync();
            var delivery = await _context.DeliveryMethods.FirstOrDefaultAsync();
            var payment = await _context.PaymentMethods.FirstOrDefaultAsync();

            if (status == null || delivery == null || payment == null)
            {
                TempData["Error"] = "Ошибка системы: справочники не заполнены.";
                return RedirectToAction("Index");
            }

            // ИСПОЛЬЗУЕМ ТРАНЗАКЦИЮ ДЛЯ БЕЗОПАСНОСТИ
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Создаем основной Заказ
                var order = new Order
                {
                    UserId = user.UserId,
                    OrderStatusId = status.OrderStatusID,
                    DeliveryMethodId = delivery.DeliveryMethodId,
                    PaymentMethodId = payment.PaymentMethodId,
                    CreatedAt = DateTime.Now,
                    TotalAmount = user.CartItems.Sum(ci => ci.Product.Price * ci.Quantity)
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Получаем Id заказа

                // 2. Обрабатываем товары: создаем OrderItems и СПИСЫВАЕМ СКЛАД
                foreach (var item in user.CartItems)
                {
                    // Находим товар в базе для обновления остатка
                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product == null) throw new Exception("Товар не найден");

                    // ПРОВЕРКА: Хватает ли товара на складе?
                    if (product.Stock < item.Quantity)
                    {
                        TempData["Error"] = $"Недостаточно товара '{product.Name}' на складе. В наличии: {product.Stock}";
                        await transaction.RollbackAsync(); // Отменяем всё, что сделали выше
                        return RedirectToAction("Index");
                    }

                    // --- ТВОЕ ТРЕБОВАНИЕ: СПИСАНИЕ ---
                    product.Stock -= item.Quantity;
                    // --------------------------------

                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    });
                }

                // 3. Очищаем корзину
                _context.CartItems.RemoveRange(user.CartItems);
                await _context.SaveChangesAsync();

                // 4. Подтверждаем транзакцию
                await transaction.CommitAsync();

                TempData["Success"] = "Заказ успешно оформлен! Товары списаны со склада.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Произошла критическая ошибка при оформлении заказа.";
                return RedirectToAction("Index");
            }
        }
    }
}