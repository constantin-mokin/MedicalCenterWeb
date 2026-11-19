using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Services;

namespace MedicalCenterWeb.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _audit;

        public ProfileController(ApplicationDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public IActionResult ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ModelState.AddModelError("", "Заполните все поля");
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Новый пароль и подтверждение не совпадают");
                return View();
            }

            if (newPassword.Length < 8
                || !newPassword.Any(char.IsUpper)
                || !newPassword.Any(char.IsLower)
                || !newPassword.Any(char.IsDigit))
            {
                ModelState.AddModelError("", "Пароль должен быть не менее 8 символов и содержать заглавные и строчные буквы, а также цифру");
                return View();
            }
            if (newPassword == currentPassword)
            {
                ModelState.AddModelError("", "Новый пароль должен отличаться от текущего");
                return View();
            }

            var userId = User.GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (user.Password != currentPassword)
            {
                ModelState.AddModelError("", "Текущий пароль неверен");
                return View();
            }

            user.Password = newPassword;
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Смена пароля", $"Пользователь {user.Username}");
            TempData["Success"] = "Пароль успешно изменён";
            return View();
        }
    }
}
