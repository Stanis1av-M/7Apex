using Apex7.Data;
using Apex7.Data.Entities;
using Apex7.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Apex7.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ВХОД (GET)
        // ==========================================
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ==========================================
        // 2. ВХОД (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Ищем пользователя и подгружаем его роль
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null)
                {
                    if (user.Ban)
                    {
                        ModelState.AddModelError("", "Ваш аккаунт заблокирован администратором.");
                        return View(model);
                    }

                    // Авторизуем пользователя
                    await Authenticate(user);

                    // Если есть адрес возврата (например, из корзины), отправляем туда
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    TempData["Success"] = $"Добро пожаловать, {user.FullName}!";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Неверный адрес почты или пароль.");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // ==========================================
        // 3. РЕГИСТРАЦИЯ (GET)
        // ==========================================
        [HttpGet]
        public IActionResult Register() => View();

        // ==========================================
        // 4. РЕГИСТРАЦИЯ (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
                if (userExists)
                {
                    ModelState.AddModelError("Email", "Этот Email уже используется в системе.");
                    return View(model);
                }

                // Ищем роль "Клиент" (ID 4 по твоей базе)
                var clientRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Клиент");

                var newUser = new User
                {
                    Email = model.Email,
                    Password = model.Password, // Внимание: в реальном проекте используй хеширование!
                    FullName = model.FullName,
                    RoleId = clientRole?.RoleId ?? 4,
                    RegistrationDate = DateTime.Now,
                    Ban = false
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Сразу авторизуем после регистрации
                await Authenticate(newUser);

                TempData["Success"] = "Регистрация завершена! Добро пожаловать.";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // ==========================================
        // 5. ВЫХОД
        // ==========================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 6. ВСПОМОГАТЕЛЬНЫЙ МЕТОД (AUTHENTICATE)
        // ==========================================
        private async Task Authenticate(User user)
        {
            // Формируем Claims (удостоверение пользователя)
            var claims = new List<Claim>
            {
                // Уникальный ID (нужен для корзины и UserController)
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), 
                
                // Email (используется как основной Name в приложении)
                new Claim(ClaimTypes.Name, user.Email), 
                
                // Полное имя (для вывода в профиле)
                new Claim("FullName", user.FullName),
                
                // Роль (Администратор, Менеджер или Клиент)
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Клиент")
            };

            var id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimTypes.Name, ClaimTypes.Role);

            // Создаем куку авторизации
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id),
                new AuthenticationProperties
                {
                    IsPersistent = true, // Запомнить пользователя
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                });
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}