using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenterWeb.Services
{
    public class AppointmentValidator
    {
        private readonly ApplicationDbContext _context;

        public AppointmentValidator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsValid, string Message)> ValidateAsync(int doctorId, DateTime startDateTime, int durationMinutes, int? excludeAppointmentId = null)
        {
            if (durationMinutes <= 0)
                return (false, "Неверная длительность процедуры.");

            var day = startDateTime.DayOfWeek;
            var time = startDateTime.TimeOfDay;
            var endTime = time + TimeSpan.FromMinutes(durationMinutes);

            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == day)
                .ToListAsync();

            if (schedules.Count == 0)
                return (false, "Для выбранного врача нет рабочего времени в этот день.");

            var covered = schedules.Any(s => time >= s.StartTime && endTime <= s.EndTime);
            if (!covered)
                return (false, "Выбранное время не попадает в рабочее время врача.");

            var dayStart = startDateTime.Date;
            var dayEnd = dayStart.AddDays(1);
            var appointments = await _context.Appointments
                .Include(a => a.Procedure)
                .Where(a => a.DoctorId == doctorId
                         && a.Status != AppointmentStatus.Cancelled
                         && a.AppointmentDateTime >= dayStart
                         && a.AppointmentDateTime < dayEnd)
                .ToListAsync();

            var newStart = startDateTime;
            var newEnd = startDateTime.AddMinutes(durationMinutes);

            foreach (var a in appointments)
            {
                if (excludeAppointmentId.HasValue && a.Id == excludeAppointmentId.Value)
                    continue;

                var aDuration = a.Procedure?.DurationMinutes ?? 30;
                var aEnd = a.AppointmentDateTime.AddMinutes(aDuration);

                if (newStart < aEnd && a.AppointmentDateTime < newEnd)
                {
                    return (false, $"Время пересекается с приёмом {a.AppointmentDateTime:dd.MM.yyyy HH:mm}.");
                }
            }

            return (true, "OK");
        }
    }
}
