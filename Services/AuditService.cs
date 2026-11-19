using System.Security.Claims;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;

namespace MedicalCenterWeb.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string details = "")
        {
            var username = _httpContextAccessor?.HttpContext?.User?.FindFirst("Username")?.Value ?? "система";
            _context.AuditLogs.Add(new AuditLog
            {
                Username = username,
                Action = action,
                Details = details,
                Timestamp = AppClock.Now
            });
            await _context.SaveChangesAsync();
        }
    }
}
