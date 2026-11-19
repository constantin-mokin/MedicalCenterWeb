using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;

namespace MedicalCenterWeb.Controllers
{
    [Authorize(Roles = "Staff,Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppointmentValidator _validator;
        private readonly AuditService _audit;

        public StaffController(ApplicationDbContext context, AppointmentValidator validator, AuditService audit)
        {
            _context = context;
            _validator = validator;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var today = AppClock.Today;
            var tomorrow = today.AddDays(1);

            var vm = new StaffDashboardViewModel
            {
                PatientCount = await _context.Patients.CountAsync(),
                AppointmentCount = await _context.Appointments.CountAsync(a => a.Status != AppointmentStatus.Cancelled),
                TodayAppointments = await _context.Appointments
                    .Include(a => a.Patient).Include(a => a.Doctor).ThenInclude(d => d.User).Include(a => a.Procedure)
                    .Where(a => a.AppointmentDateTime >= today && a.AppointmentDateTime < tomorrow && a.Status != AppointmentStatus.Cancelled)
                    .OrderBy(a => a.AppointmentDateTime).Take(10).ToListAsync(),
                TodayDate = today.ToString("D")
            };
            return View(vm);
        }


        public async Task<IActionResult> Patients(string search, int page = 1)
        {
            const int pageSize = 15;
            page = Paging.Normalize(page);
            var query = _context.Patients.AsQueryable();

            List<Patient>? matches = null;
            if (CiSearch.HasQuery(search))
            {
                var needle = CiSearch.Normalize(search);
                matches = (await query.ToListAsync())
                    .Where(p => CiSearch.Normalize(p.FirstName).Contains(needle)
                             || CiSearch.Normalize(p.LastName).Contains(needle)
                             || CiSearch.Normalize(p.Phone).Contains(needle))
                    .OrderBy(p => p.LastName)
                    .ToList();
            }

            var total = matches?.Count ?? await query.CountAsync();
            var list = matches != null
                ? matches.Skip((page - 1) * pageSize).Take(pageSize).ToList()
                : await query.OrderBy(p => p.LastName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new PatientsListViewModel
            {
                Patients = list,
                CurrentPage = page,
                TotalPages = Paging.TotalPages(total, pageSize),
                Search = search ?? string.Empty
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPatient(string firstName, string lastName, string phone, string email, DateTime dateOfBirth, Gender gender)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(phone))
            {
                TempData["Error"] = "Заполните имя, фамилию и телефон";
                return RedirectToAction("Patients");
            }

            var normalizedPhone = phone.Trim();
            if (await _context.Patients.AnyAsync(p => p.Phone == normalizedPhone))
            {
                TempData["Error"] = $"Пациент с телефоном {normalizedPhone} уже зарегистрирован";
                return RedirectToAction("Patients");
            }
            if (dateOfBirth > AppClock.Today)
            {
                TempData["Error"] = "Дата рождения не может быть в будущем";
                return RedirectToAction("Patients");
            }

            _context.Patients.Add(new Patient
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Phone = normalizedPhone,
                Email = email?.Trim() ?? string.Empty,
                DateOfBirth = dateOfBirth,
                Gender = gender,
                MedicalHistory = string.Empty
            });
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Добавление пациента", $"{firstName} {lastName}");
            TempData["Success"] = "Пациент добавлен";
            return RedirectToAction("Patients");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePatient(int id, string firstName, string lastName, string phone, string email, DateTime dateOfBirth, Gender gender)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            var normalizedPhone = phone.Trim();
            if (await _context.Patients.AnyAsync(p => p.Phone == normalizedPhone && p.Id != id))
            {
                TempData["Error"] = $"Телефон {normalizedPhone} уже закреплён за другим пациентом";
                return RedirectToAction("Patients");
            }

            patient.FirstName = firstName.Trim();
            patient.LastName = lastName.Trim();
            patient.Phone = normalizedPhone;
            patient.Email = email?.Trim() ?? string.Empty;
            patient.DateOfBirth = dateOfBirth;
            patient.Gender = gender;
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Обновление пациента", $"{firstName} {lastName}");
            TempData["Success"] = "Данные пациента обновлены";
            return RedirectToAction("Patients");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            var hasAppointments = await _context.Appointments.AnyAsync(a => a.PatientId == id);
            if (hasAppointments)
            {
                TempData["Error"] = $"Нельзя удалить пациента «{patient.FirstName} {patient.LastName}»: в базе есть его приёмы и медкарты. Сохранность медицинской истории обязательна.";
                return RedirectToAction("Patients");
            }

            var name = $"{patient.FirstName} {patient.LastName}";
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Удаление пациента", name);
            TempData["Success"] = "Пациент удалён";
            return RedirectToAction("Patients");
        }

        public async Task<IActionResult> PatientHistory(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            var appointments = await _context.Appointments.Include(a => a.Doctor).ThenInclude(d => d.User).Include(a => a.Procedure).Include(a => a.MedicalRecord)
                .Where(a => a.PatientId == id).OrderByDescending(a => a.AppointmentDateTime).ToListAsync();

            var vm = new PatientHistoryViewModel { Patient = patient, Appointments = appointments };
            return View(vm);
        }


        public async Task<IActionResult> Appointments(string search, int? statusFilter, int? doctorFilter, DateTime? dateFrom, DateTime? dateTo, int page = 1)
        {
            const int pageSize = 15;
            page = Paging.Normalize(page);
            var query = _context.Appointments.Include(a => a.Patient).Include(a => a.Doctor).ThenInclude(d => d.User).Include(a => a.Procedure).AsQueryable();
            query = query.Where(a => a.Status != AppointmentStatus.Cancelled);
            if (statusFilter.HasValue) query = query.Where(a => a.Status == (AppointmentStatus)statusFilter.Value);
            if (doctorFilter.HasValue) query = query.Where(a => a.DoctorId == doctorFilter.Value);

            if (dateFrom.HasValue) query = query.Where(a => a.AppointmentDateTime >= dateFrom.Value.Date);
            if (dateTo.HasValue) query = query.Where(a => a.AppointmentDateTime < dateTo.Value.Date.AddDays(1));

            List<Appointment>? matches = null;
            if (CiSearch.HasQuery(search))
            {
                var needle = CiSearch.Normalize(search);
                matches = (await query.ToListAsync())
                    .Where(a => CiSearch.Normalize(a.Patient.FirstName + " " + a.Patient.LastName).Contains(needle))
                    .ToList();
            }

            var total = matches?.Count ?? await query.CountAsync();
            var list = matches != null
                ? matches.OrderBy(a => a.AppointmentDateTime).Skip((page - 1) * pageSize).Take(pageSize).ToList()
                : await query.OrderBy(a => a.AppointmentDateTime).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new AppointmentsPageViewModel
            {
                Appointments = list,
                Patients = await _context.Patients.OrderBy(p => p.LastName).ToListAsync(),
                Doctors = await _context.Doctors.Include(d => d.User).ToListAsync(),
                Procedures = await _context.Procedures.OrderBy(p => p.Name).ToListAsync(),
                CurrentPage = page,
                TotalPages = Paging.TotalPages(total, pageSize),
                Search = search ?? string.Empty,
                StatusFilter = statusFilter,
                DoctorFilter = doctorFilter,
                DateFrom = dateFrom,
                DateTo = dateTo
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAppointment(int patientId, int doctorId, int procedureId, DateTime appointmentDate, TimeSpan appointmentTime)
        {
            if (patientId <= 0 || doctorId <= 0 || procedureId <= 0)
            {
                TempData["Error"] = "Выберите пациента, врача и процедуру";
                return RedirectToAction("Appointments");
            }
            var appointmentDateTime = appointmentDate.Date + appointmentTime;
            if (appointmentDateTime < AppClock.Now)
            {
                TempData["Error"] = "Невозможно создать приём в прошлом";
                return RedirectToAction("Appointments");
            }
            var proc = await _context.Procedures.FindAsync(procedureId);
            var duration = proc?.DurationMinutes ?? 30;
            var (isValid, message) = await _validator.ValidateAsync(doctorId, appointmentDateTime, duration, null);
            if (!isValid)
            {
                TempData["Error"] = $"Невозможно записать: {message}";
                return RedirectToAction("Appointments");
            }

            _context.Appointments.Add(new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                ProcedureId = procedureId,
                AppointmentDateTime = appointmentDateTime,
                Status = AppointmentStatus.Scheduled,
                RecordedPrice = proc?.Price
            });
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Запись на приём", $"Пациент: {patientId}, Врач: {doctorId}");
            TempData["Success"] = "Приём записан";
            return RedirectToAction("Appointments");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointment(int id, int patientId, int doctorId, int procedureId, DateTime appointmentDate, TimeSpan appointmentTime)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();
            var appointmentDateTime = appointmentDate.Date + appointmentTime;
            var proc = await _context.Procedures.FindAsync(procedureId);
            var duration = proc?.DurationMinutes ?? 30;
            var (isValid, message) = await _validator.ValidateAsync(doctorId, appointmentDateTime, duration, id);
            if (!isValid)
            {
                TempData["Error"] = $"Невозможно обновить: {message}";
                return RedirectToAction("Appointments");
            }

            appt.PatientId = patientId;
            appt.DoctorId = doctorId;
            appt.ProcedureId = procedureId;
            appt.AppointmentDateTime = appointmentDateTime;
            appt.RecordedPrice = proc?.Price;
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Изменение приёма", $"ID: {id}");
            TempData["Success"] = "Приём обновлён";
            return RedirectToAction("Appointments");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();
            appt.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Отмена приёма", $"ID: {id}");
            TempData["Success"] = "Приём отменён";
            return RedirectToAction("Appointments");
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var appt = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null) return NotFound();
            if (appt.Status != AppointmentStatus.Scheduled)
            {
                TempData["Error"] = "Приём уже закрыт или отменён";
                return RedirectToAction("Appointments");
            }

            appt.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();

            var patientName = $"{appt.Patient?.LastName} {appt.Patient?.FirstName}".Trim();
            await _audit.LogAsync("Отметка выполнения приёма (администратор)", $"Приём #{appt.Id}, пациент: {patientName}");

            TempData["Success"] = "Приём помечен как выполненный";
            return RedirectToAction("Appointments");
        }
    }
}
