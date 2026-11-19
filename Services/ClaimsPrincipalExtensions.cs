using System.Security.Claims;

namespace MedicalCenterWeb.Services
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }

        public static string GetDisplayName(this ClaimsPrincipal principal) =>
            principal.Identity?.Name ?? string.Empty;
    }
}
