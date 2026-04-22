using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apex7.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ВИТРИНА МАГАЗИНА
        // ==========================================
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .AsQueryable();

            // Логика видимости: скрываем товары для клиентов
            if (!User.IsInRole("Администратор") && !User.IsInRole("Менеджер"))
            {
                query = query.Where(p => p.IsVisible);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || p.Article.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", categoryId);

            ViewBag.CurrentSearch = searchString;
            ViewBag.SelectedCategory = categoryId;

            var products = await query.ToListAsync();
            return View(products);
        }

        // ==========================================
        // 2. СОЗДАНИЕ ТОВАРА (Только Админ)
        // ==========================================
        [Authorize(Roles = "Администратор")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadViewBags();
            return View(new Product { IsVisible = true, Stock = 1 });
        }

        [Authorize(Roles = "Администратор")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {
            // Валидация цен: Старая цена должна быть БОЛЬШЕ текущей
            ValidatePrices(model);

            if (!ModelState.IsValid)
            {
                await LoadViewBags(model);
                return View(model);
            }

            // Расчет скидки и загрузка фото
            CalculateDiscount(model);
            if (imageFile != null) model.ImageUrl = await SaveImage(imageFile);
            else model.ImageUrl = "/images/products/default.jpg";

            model.CreatedAt = DateTime.Now;
            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Товар '{model.Name}' успешно создан!";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 3. РЕДАКТИРОВАНИЕ ТОВАРА
        // ==========================================
        [Authorize(Roles = "Менеджер, Администратор")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            await LoadViewBags(product);
            return View(product);
        }

        [Authorize(Roles = "Менеджер, Администратор")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product model, IFormFile? imageFile)
        {
            ValidatePrices(model);

            if (!ModelState.IsValid)
            {
                await LoadViewBags(model);
                TempData["Error"] = "Ошибка! Проверьте введенные данные.";
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) return NotFound();

            // Обновление полей
            product.Name = model.Name;
            product.Article = model.Article;
            product.Price = model.Price;
            product.OldPrice = model.OldPrice;
            CalculateDiscount(product); // Считаем скидку для объекта из БД
            product.Stock = model.Stock;
            product.Description = model.Description;
            product.IsVisible = model.IsVisible;
            product.CategoryId = model.CategoryId;
            product.ManufacturerId = model.ManufacturerId;
            product.SupplierId = model.SupplierId;

            if (imageFile != null) product.ImageUrl = await SaveImage(imageFile);

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Товар '{product.Name}' обновлен.";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 4. УДАЛЕНИЕ ТОВАРА
        // ==========================================
        [Authorize(Roles = "Менеджер, Администратор")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.OrderItems)
                .Include(p => p.CartItems)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            if (product.OrderItems.Any())
            {
                TempData["Error"] = "Нельзя удалить товар, который уже заказывали. Скройте его.";
                return RedirectToAction("Edit", new { id = id });
            }

            if (product.CartItems.Any()) _context.CartItems.RemoveRange(product.CartItems);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Товар удален.";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ==========================================

        private async Task LoadViewBags(Product? product = null)
        {
            var categories = await _context.Categories.ToListAsync();
            var manufacturers = await _context.Manufacturers.ToListAsync();
            var suppliers = await _context.Suppliers.ToListAsync();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", product?.CategoryId);
            ViewBag.Manufacturers = new SelectList(manufacturers, "ManufactureId", "Name", product?.ManufacturerId);
            ViewBag.Suppliers = new SelectList(suppliers, "SupplierId", "Name", product?.SupplierId);
        }

        private void ValidatePrices(Product model)
        {
            if (model.OldPrice.HasValue && model.OldPrice > 0 && model.OldPrice <= model.Price)
            {
                ModelState.AddModelError("OldPrice", "Старая цена должна быть выше текущей (цена до скидки).");
            }
        }

        private void CalculateDiscount(Product model)
        {
            if (model.OldPrice.HasValue && model.OldPrice > model.Price)
            {
                model.Discount = (int)(100 - (model.Price * 100 / model.OldPrice.Value));
            }
            else
            {
                model.Discount = 0;
            }
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/images/products/" + fileName;
        }
    }
}