using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apex7.Controllers
{
    [Authorize(Roles = "Администратор, Менеджер")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}