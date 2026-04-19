using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;

namespace Apex7.Controllers
{
    public class ToursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ToursController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
           
            var tours = await _context.Tours
                .Include(t => t.ComplexityLevel)
                .Include(t => t.TourGroups)
                .ToListAsync();

            return View(tours);
        }
    }
}