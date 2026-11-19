using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalCenterWeb.Models;

namespace MedicalCenterWeb.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login", "Auth");

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            return role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Doctor" => RedirectToAction("Index", "Doctor"),
                "Staff" => RedirectToAction("Index", "Staff"),
                _ => RedirectToAction("Login", "Auth")
            };
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult AccessDenied() => View();
    }
}
