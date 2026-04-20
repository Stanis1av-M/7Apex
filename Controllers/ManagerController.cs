using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;

namespace Apex7.Controllers
{
    [Authorize(Roles = "Менеджер, Администратор")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ManagerController(ApplicationDbContext context) => _context = context;

        // Список всех заказов для менеджера
        public async Task<IActionResult> Orders()
        {
            
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderStatus)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Statuses = await _context.OrderStatuses.ToListAsync(); 
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, int statusId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatusId = statusId;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Статус заказа обновлен!";
            }
            return RedirectToAction("Orders");
        }
    }
}