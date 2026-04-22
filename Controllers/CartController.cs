using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;
using System.Security.Claims;

namespace Apex7.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context) => _context = context;

        // ==========================================
        // 1. ОТОБРАЖЕНИЕ КОРЗИНЫ
        // ==========================================
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                TempData["Error"] = "Пользователь не найден. Пожалуйста, перевойдите в систему.";
                return RedirectToAction("Login", "Account");
            }

            return View(user.CartItems.ToList());
        }

        // ==========================================
        // 2. ДОБАВЛЕНИЕ С ПРОВЕРКОЙ ОСТАТКА
        // ==========================================
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Add(int productId)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                TempData["Error"] = "Для добавления товаров необходимо войти в систему!";
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.CartItems)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var product = await _context.Products.FindAsync(productId);

            if (user == null || product == null)
            {
                TempData["Error"] = "Ошибка при добавлении товара.";
                return RedirectToAction("Index", "Shop");
            }

            // ПРОВЕРКА: Есть ли вообще товар на складе?
            if (product.Stock <= 0)
            {
                TempData["Error"] = "К сожалению, этот товар закончился.";
                return RedirectToAction("Index", "Shop");
            }

            var existingItem = user.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (existingItem != null)
            {
                // ПРОВЕРКА: Не пытаемся ли мы добавить больше, чем есть на складе?
                if (existingItem.Quantity + 1 > product.Stock)
                {
                    TempData["Error"] = $"Вы не можете добавить больше {product.Stock} шт. (всё, что есть на складе)";
                    return RedirectToAction("Index");
                }
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
            TempData["Success"] = "Товар добавлен в корзину!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Товар удален.";
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. ОФОРМЛЕНИЕ С ЗАЩИТОЙ ОТ МИНУСА
        // ==========================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || !user.CartItems.Any()) return RedirectToAction("Index");

            var status = await _context.OrderStatuses.FirstOrDefaultAsync();
            var delivery = await _context.DeliveryMethods.FirstOrDefaultAsync();
            var payment = await _context.PaymentMethods.FirstOrDefaultAsync();

            if (status == null || delivery == null || payment == null)
            {
                TempData["Error"] = "Справочники БД не заполнены.";
                return RedirectToAction("Index");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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
                await _context.SaveChangesAsync();

                foreach (var item in user.CartItems)
                {
                    // Находим актуальный товар в базе для проверки остатка
                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product == null) continue;

                    // КРИТИЧЕСКАЯ ПРОВЕРКА: Если товара не хватает, отменяем всё!
                    if (product.Stock < item.Quantity)
                    {
                        TempData["Error"] = $"Ошибка: товара '{product.Name}' недостаточно. В наличии: {product.Stock} шт.";
                        await transaction.RollbackAsync(); // ОТМЕНА ВСЕХ ИЗМЕНЕНИЙ
                        return RedirectToAction("Index");
                    }

                    // Списание со склада (теперь безопасно)
                    product.Stock -= item.Quantity;

                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    });
                }

                _context.CartItems.RemoveRange(user.CartItems);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // ПОДТВЕРЖДЕНИЕ

                TempData["Success"] = "Заказ успешно оформлен!";
                return RedirectToAction("MyOrders", "User");
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Произошла ошибка при оформлении заказа.";
                return RedirectToAction("Index");
            }
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            if (claim != null && int.TryParse(claim.Value, out int id)) return id;

            var email = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            return user?.UserId ?? 0;
        }
    }
}