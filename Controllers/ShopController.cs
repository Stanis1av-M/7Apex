using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Apex7.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Основной метод магазина (доступен всем)
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            // 1. Начинаем запрос
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .AsQueryable();

            // 2. ЛОГИКА ВИДИМОСТИ: 
            // Если пользователь НЕ админ и НЕ менеджер — прячем скрытые товары совсем
            if (!User.IsInRole("Администратор") && !User.IsInRole("Менеджер"))
            {
                query = query.Where(p => p.IsVisible);
            }

            // 3. Поиск и категории (оставляем как было)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || p.Article.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CurrentSearch = searchString;

            var products = await query.ToListAsync();
            return View(products);
        }
        // --- МЕТОДЫ ДЛЯ МЕНЕДЖЕРА ---

        [Authorize(Roles = "Менеджер, Администратор")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            // ОБЯЗАТЕЛЬНО ДЛЯ ВЫПАДАЮЩЕГО СПИСКА:
            ViewBag.Manufacturers = await _context.Manufacturers.ToListAsync();

            return View(product);
        }

        [Authorize(Roles = "Менеджер, Администратор")]
        [HttpPost]
        public async Task<IActionResult> Edit(Product model, IFormFile? imageFile)
        {
            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) return NotFound();

            // Обновляем ВСЕ данные, которые есть в форме
            product.Name = model.Name;
            product.Article = model.Article; // Добавлено
            product.Price = model.Price;
            product.OldPrice = model.OldPrice; // Добавлено
            product.Discount = model.Discount; // Добавлено
            product.Stock = model.Stock;
            product.Description = model.Description; // Добавлено
            product.IsVisible = model.IsVisible;
            product.CategoryId = model.CategoryId;
            product.ManufacturerId = model.ManufacturerId;

            // Логика загрузки нового фото
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                product.ImageUrl = "/images/products/" + fileName;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Товар успешно обновлен!";
            return RedirectToAction("Index");
        }

        // Логика удаления товара
        [Authorize(Roles = "Менеджер, Администратор")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Проверяем, не купил ли кто-то этот товар (защита базы)
            var product = await _context.Products
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            // Запрещаем удаление, если товар есть в заказах
            if (product.OrderItems.Any())
            {
                TempData["Error"] = "ОШИБКА: Этот товар нельзя удалить, так как он присутствует в истории заказов. Вы можете скрыть его, сняв галочку 'Товар виден на сайте'.";
                return RedirectToAction("Edit", new { id = product.ProductId });
            }

            // Если заказов нет — безопасно удаляем
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Товар навсегда удален из базы!";
            return RedirectToAction("Index");
        }
    }
}