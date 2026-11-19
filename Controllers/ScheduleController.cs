using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;

namespace MedicalCenterWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _audit;

        public ScheduleController(ApplicationDbContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new ScheduleIndexViewModel
            {
                Doctors = await _context.Doctors.Include(d => d.User).OrderBy(d => d.User.LastName).ToListAsync()
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedules(int doctorId)
        {
            var list = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId)
                .ToListAsync();

            var sorted = list
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToList();

            return Json(sorted);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int doctorId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime <= startTime)
            {
                TempData["Error"] = "Время окончания должно быть позже начала";
                return RedirectToAction("Index");
            }

            var existing = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek)
                .ToListAsync();

            var hasConflict = existing.Any(s => s.StartTime < endTime && s.EndTime > startTime);
            if (hasConflict)
            {
                TempData["Error"] = "Новое расписание пересекается с существующим";
                return RedirectToAction("Index");
            }

            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctorId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime
            });
            await _context.SaveChangesAsync();

            var doctorName = await GetDoctorNameAsync(doctorId);
            await _audit.LogAsync("Добавление расписания", $"Врач: {doctorName}, день: {GetDayName(dayOfWeek)}, {startTime:hh\\:mm}–{endTime:hh\\:mm}");
            TempData["Success"] = "Расписание добавлено";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            if (endTime <= startTime)
            {
                TempData["Error"] = "Время окончания должно быть позже начала";
                return RedirectToAction("Index");
            }

            var others = await _context.DoctorSchedules
                .Where(s => s.DoctorId == schedule.DoctorId && s.DayOfWeek == dayOfWeek && s.Id != id)
                .ToListAsync();

            var hasConflict = others.Any(s => s.StartTime < endTime && s.EndTime > startTime);
            if (hasConflict)
            {
                TempData["Error"] = "Обновлённое расписание пересекается с существующим";
                return RedirectToAction("Index");
            }

            schedule.DayOfWeek = dayOfWeek;
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;
            await _context.SaveChangesAsync();

            var doctorName = await GetDoctorNameAsync(schedule.DoctorId);
            await _audit.LogAsync("Изменение расписания", $"Врач: {doctorName}, день: {GetDayName(dayOfWeek)}, {startTime:hh\\:mm}–{endTime:hh\\:mm}");
            TempData["Success"] = "Расписание обновлено";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            var doctorName = await GetDoctorNameAsync(schedule.DoctorId);
            await _audit.LogAsync("Удаление расписания", $"Врач: {doctorName}, день: {GetDayName(schedule.DayOfWeek)}");
            TempData["Success"] = "Расписание удалено";
            return RedirectToAction("Index");
        }

        private async Task<string> GetDoctorNameAsync(int doctorId) =>
            await _context.Doctors
                .Where(d => d.Id == doctorId)
                .Select(d => d.User.LastName + " " + d.User.FirstName)
                .FirstOrDefaultAsync() ?? "неизвестно";

        private static string GetDayName(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => "понедельник",
            DayOfWeek.Tuesday => "вторник",
            DayOfWeek.Wednesday => "среда",
            DayOfWeek.Thursday => "четверг",
            DayOfWeek.Friday => "пятница",
            DayOfWeek.Saturday => "суббота",
            _ => "воскресенье"
        };
    }
}
