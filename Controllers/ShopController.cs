using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities; // Обязательно добавь для Product
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
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Where(p => p.IsVisible);

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
            ViewBag.SelectedCategory = categoryId;

            var products = await query.ToListAsync();
            return View(products);
        }



        [Authorize(Roles = "Менеджер, Администратор")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

           
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Manufacturers = await _context.Manufacturers.ToListAsync();

            return View(product);
        }

        [Authorize(Roles = "Менеджер, Администратор")]
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Edit(Product model, IFormFile? imageFile)
        {
            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) return NotFound();

            // Обновляем данные
            product.Name = model.Name;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.IsVisible = model.IsVisible;
            product.CategoryId = model.CategoryId;           
            product.ManufacturerId = model.ManufacturerId; 


            
            // Логика загрузки фото
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
            TempData["Success"] = "Товар обновлен!";
            return RedirectToAction("Index");
        }
    }
}