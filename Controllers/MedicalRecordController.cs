using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;

namespace MedicalCenterWeb.Controllers
{
    [Authorize(Roles = "Doctor,Admin")]
    public class MedicalRecordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _audit;

        public MedicalRecordController(ApplicationDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public async Task<IActionResult> Edit(int appointmentId)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Procedure)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appt == null) return NotFound();

            if (!await CanAccessAppointmentAsync(appt)) return Forbid();

            var record = await _context.MedicalRecords
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);

            return View(new MedicalRecordEditViewModel { Appointment = appt, Record = record });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int appointmentId, string diagnosis, string chronicDiseases, string recommendations, string notes)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appt == null) return NotFound();

            if (!await CanAccessAppointmentAsync(appt)) return Forbid();

            var record = await _context.MedicalRecords
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);

            if (record == null)
            {
                record = new MedicalRecord
                {
                    AppointmentId = appointmentId,
                    Diagnosis = diagnosis?.Trim() ?? string.Empty,
                    ChronicDiseases = chronicDiseases?.Trim() ?? string.Empty,
                    Recommendations = recommendations?.Trim() ?? string.Empty,
                    Notes = notes?.Trim() ?? string.Empty,
                    CreatedAt = AppClock.Now
                };
                _context.MedicalRecords.Add(record);
            }
            else
            {
                record.Diagnosis = diagnosis?.Trim() ?? string.Empty;
                record.ChronicDiseases = chronicDiseases?.Trim() ?? string.Empty;
                record.Recommendations = recommendations?.Trim() ?? string.Empty;
                record.Notes = notes?.Trim() ?? string.Empty;
                record.UpdatedAt = AppClock.Now;
            }

            await _context.SaveChangesAsync();

            var patientName = $"{appt.Patient?.LastName} {appt.Patient?.FirstName}".Trim();
            await _audit.LogAsync("Сохранение медкарты", $"Приём #{appointmentId}, пациент: {patientName}");

            TempData["Success"] = "Медкарта сохранена";
            return RedirectToAction("Edit", new { appointmentId });
        }

        private async Task<bool> CanAccessAppointmentAsync(Appointment appt)
        {
            if (User.IsInRole("Admin"))
                return true;

            if (User.IsInRole("Doctor"))
            {
                var userId = User.GetUserId();
                return await _context.Doctors.AnyAsync(d => d.UserId == userId && d.Id == appt.DoctorId);
            }

            return false;
        }
    }
}
