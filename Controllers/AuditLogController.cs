using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;

namespace MedicalCenterWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _audit;

        public AuditLogController(ApplicationDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public async Task<IActionResult> Index(string search, int page = 1)
        {
            const int pageSize = 20;
            page = Paging.Normalize(page);
            var query = _context.AuditLogs.AsQueryable();

            List<AuditLog>? matches = null;
            if (CiSearch.HasQuery(search))
            {
                var needle = CiSearch.Normalize(search);
                matches = (await query.ToListAsync())
                    .Where(l => CiSearch.Normalize(l.Username).Contains(needle)
                             || CiSearch.Normalize(l.Action).Contains(needle)
                             || CiSearch.Normalize(l.Details).Contains(needle))
                    .ToList();
            }

            var total = matches?.Count ?? await query.CountAsync();
            var logs = matches != null
                ? matches.OrderByDescending(l => l.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToList()
                : await query
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            var vm = new AuditLogListViewModel
            {
                Logs = logs,
                CurrentPage = page,
                TotalPages = Paging.TotalPages(total, pageSize),
                Search = search ?? string.Empty,
                Total = total
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanOld()
        {
            var cutoff = AppClock.Now.AddDays(-90);
            var old = await _context.AuditLogs.Where(l => l.Timestamp < cutoff).ToListAsync();
            if (old.Count > 0)
            {
                _context.AuditLogs.RemoveRange(old);
                await _context.SaveChangesAsync();
            }
            await _audit.LogAsync("Очистка журнала аудита", $"Удалено записей: {old.Count} (старше {cutoff:dd.MM.yyyy})");
            TempData["Success"] = $"Журнал очищен: удалено {old.Count} записей старше 90 дней";
            return RedirectToAction("Index");
        }
    }
}
