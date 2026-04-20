using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Apex7.Data;
using Apex7.Data.Entities;

namespace Apex7.Controllers
{
    // Добавляем атрибут авторизации на весь контроллер
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        public FeedbackController(ApplicationDbContext context) => _context = context;

        // Снимаем авторизацию для GET-запроса, чтобы гость мог увидеть страницу,
        // но кнопка отправки будет защищена логикой в View
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Feedback model)
        {
           
            if (!User.Identity!.IsAuthenticated)
            {
               
                TempData["FeedbackData"] = model.Message;
                TempData["FeedbackName"] = model.Name;
                TempData["FeedbackEmail"] = model.Email;

                return RedirectToAction("Login", "Account", new
                {
                    message = "auth_required",
                    returnUrl = "/Feedback/Create"
                });
            }

            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                _context.Feedbacks.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ваше сообщение отправлено!";
                return RedirectToAction("Create");
            }
            return View(model);
        }
    }
}