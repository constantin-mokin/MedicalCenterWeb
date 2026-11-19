using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;
using AppAuthService = MedicalCenterWeb.Services.AuthenticationService;

namespace MedicalCenterWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppAuthService _authService;
        private readonly AuditService _audit;

        public AuthController(AppAuthService authService, AuditService audit)
        {
            _authService = authService;
            _audit = audit;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToDashboard();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Введите логин и пароль");
                return View();
            }

            var user = await _authService.LoginAsync(username, password);
            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("Username", user.Username)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });

            await _audit.LogAsync("Вход в систему", $"Пользователь: {user.Username}");

            return RedirectToDashboard();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _audit.LogAsync("Выход из системы");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private IActionResult RedirectToDashboard()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Doctor" => RedirectToAction("Index", "Doctor"),
                "Staff" => RedirectToAction("Index", "Staff"),
                _ => RedirectToAction("Login")
            };
        }
    }
}
