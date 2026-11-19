using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;

namespace MedicalCenterWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalDoctors = await _context.Doctors.CountAsync();
            var totalPatients = await _context.Patients.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();
            var completedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);
            var scheduledAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Scheduled);
            var cancelledAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled);

            var totalRevenue = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .Include(a => a.Procedure)
                .SumAsync(a => a.RecordedPrice != null ? a.RecordedPrice.Value : (a.Procedure != null ? a.Procedure.Price : 0));

            var today = AppClock.Today;
            var weekAgo = today.AddDays(-7);
            var weekAppointments = await _context.Appointments
                .Where(a => a.AppointmentDateTime >= weekAgo && a.AppointmentDateTime <= today)
                .GroupBy(a => a.AppointmentDateTime.Date)
                .Select(g => new DateCountItem { Date = g.Key.ToString("dd.MM"), Count = g.Count() })
                .ToListAsync();

            var topDoctors = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .GroupBy(a => new { a.DoctorId, DoctorName = a.Doctor.User.FirstName + " " + a.Doctor.User.LastName, Spec = a.Doctor.Specialization })
                .Select(g => new DoctorStatItem { DoctorName = g.Key.DoctorName, Specialization = g.Key.Spec, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var popularProcedures = await _context.Appointments
                .Include(a => a.Procedure)
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .GroupBy(a => new { a.ProcedureId, a.Procedure.Name })
                .Select(g => new ProcedureStatItem { ProcedureName = g.Key.Name, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var vm = new StatsIndexViewModel
            {
                TotalDoctors = totalDoctors,
                TotalPatients = totalPatients,
                TotalAppointments = totalAppointments,
                CompletedAppointments = completedAppointments,
                ScheduledAppointments = scheduledAppointments,
                CancelledAppointments = cancelledAppointments,
                TotalRevenue = totalRevenue,
                WeekAppointments = weekAppointments,
                TopDoctors = topDoctors,
                PopularProcedures = popularProcedures
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string period = "week")
        {
            var today = AppClock.Today;
            DateTime start;
            if (period == "month") start = today.AddDays(-30);
            else if (period == "year") start = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
            else start = today.AddDays(-7);

            if (period == "year")
            {
                var byMonth = await _context.Appointments
                    .Where(a => a.AppointmentDateTime >= start)
                    .GroupBy(a => new { a.AppointmentDateTime.Year, a.AppointmentDateTime.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .ToListAsync();

                var result = new List<DateCountItem>();
                for (var i = 11; i >= 0; i--)
                {
                    var month = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                    var count = byMonth.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Count ?? 0;
                    result.Add(new DateCountItem { Date = month.ToString("MM.yyyy"), Count = count });
                }
                return Json(result);
            }
            else
            {
                var byDay = await _context.Appointments
                    .Where(a => a.AppointmentDateTime >= start)
                    .GroupBy(a => a.AppointmentDateTime.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .ToListAsync();

                var counts = byDay.ToDictionary(x => x.Date, x => x.Count);
                var days = period == "month" ? 31 : 8;

                var result = new List<DateCountItem>();
                for (var i = days - 1; i >= 0; i--)
                {
                    var day = today.AddDays(-i);
                    result.Add(new DateCountItem
                    {
                        Date = day.ToString("dd.MM"),
                        Count = counts.TryGetValue(day, out var count) ? count : 0
                    });
                }
                return Json(result);
            }
        }
    }
}
