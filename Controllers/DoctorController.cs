using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;

namespace MedicalCenterWeb.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _audit;

        public DoctorController(ApplicationDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        private async Task<int> GetCurrentDoctorIdAsync()
        {
            var userId = User.GetUserId();
            return await _context.Doctors
                .Where(d => d.UserId == userId)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<IActionResult> Index(string search, int page = 1)
        {
            const int pageSize = 15;
            page = Paging.Normalize(page);
            var doctorId = await GetCurrentDoctorIdAsync();
            if (doctorId == 0) return RedirectToAction("Login", "Auth");

            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null) return RedirectToAction("Login", "Auth");

            var query = _context.Appointments.Include(a => a.Patient).Include(a => a.Procedure)
                .Where(a => a.DoctorId == doctorId && a.Status != AppointmentStatus.Cancelled);

            List<Appointment>? matches = null;
            if (CiSearch.HasQuery(search))
            {
                var needle = CiSearch.Normalize(search);
                matches = (await query.ToListAsync())
                    .Where(a => CiSearch.Normalize(a.Patient.FirstName + " " + a.Patient.LastName).Contains(needle))
                    .ToList();
            }

            var total = matches?.Count ?? await query.CountAsync();
            var appointments = matches != null
                ? matches.OrderBy(a => a.AppointmentDateTime).Skip((page - 1) * pageSize).Take(pageSize).ToList()
                : await query.OrderBy(a => a.AppointmentDateTime)
                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new DoctorAppointmentsViewModel
            {
                Doctor = doctor,
                Appointments = appointments,
                CurrentPage = page,
                TotalPages = Paging.TotalPages(total, pageSize),
                Search = search ?? string.Empty
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null) return NotFound();
            if (appt.DoctorId != doctorId) return Forbid();
            if (appt.Status != AppointmentStatus.Scheduled)
            {
                TempData["Error"] = "Приём уже закрыт или отменён";
                return RedirectToAction("Index");
            }

            appt.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();

            var patientName = $"{appt.Patient?.LastName} {appt.Patient?.FirstName}".Trim();
            await _audit.LogAsync("Отметка выполнения приёма", $"Приём #{appt.Id}, пациент: {patientName}");

            TempData["Success"] = "Приём помечен как выполненный";
            return RedirectToAction("Index");
        }
    }
}
