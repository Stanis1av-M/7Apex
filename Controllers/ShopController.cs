using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Apex7.Controllers
{
    // Обязательно: : Controller (это дает доступ к ViewBag и View)
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Конструктор (внедряет базу данных в контроллер)
        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Основной метод страницы магазина
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            // 1. Получаем базу запроса
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .Where(p => p.IsVisible);

            // 2. Логика поиска
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || p.Article.Contains(searchString));
            }

            // 3. Фильтр по категории
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // 4. Загружаем список категорий для выпадающего списка
            ViewBag.Categories = await _context.Categories.ToListAsync();

            // Сохраняем текущий поиск, чтобы он не пропадал из поля ввода
            ViewBag.CurrentSearch = searchString;

            // 5. Выполняем запрос и отправляем в View
            var products = await query.ToListAsync();
            return View(products);
        }
    }
}