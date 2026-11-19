using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Data;
using MedicalCenterWeb.Models;
using MedicalCenterWeb.Services;
using AppAuthService = MedicalCenterWeb.Services.AuthenticationService;

namespace MedicalCenterWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppAuthService _authService;
        private readonly AuditService _audit;

        public AdminController(ApplicationDbContext context, AppAuthService authService, AuditService audit)
        {
            _context = context;
            _authService = authService;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var today = AppClock.Today;
            var tomorrow = today.AddDays(1);
            var doctorCount = await _context.Doctors.CountAsync();
            var patientCount = await _context.Patients.CountAsync();
            var appointmentCount = await _context.Appointments.CountAsync(a => a.Status != AppointmentStatus.Cancelled);
            var procedureCount = await _context.Procedures.CountAsync();
            var medicalRecordCount = await _context.MedicalRecords.CountAsync();

            var revenue = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .Include(a => a.Procedure)
                .SumAsync(a => a.RecordedPrice != null ? a.RecordedPrice.Value : (a.Procedure != null ? a.Procedure.Price : 0));

            var recent = await _context.Appointments.Include(a => a.Patient).Include(a => a.Doctor).ThenInclude(d => d.User).Include(a => a.Procedure)
                .OrderByDescending(a => a.Id).Take(5).ToListAsync();

            var todayAppointments = await _context.Appointments
                .Include(a => a.Patient).Include(a => a.Doctor).ThenInclude(d => d.User).Include(a => a.Procedure)
                .Where(a => a.AppointmentDateTime >= today && a.AppointmentDateTime < tomorrow && a.Status != AppointmentStatus.Cancelled)
                .OrderBy(a => a.AppointmentDateTime).ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                DoctorCount = doctorCount,
                PatientCount = patientCount,
                AppointmentCount = appointmentCount,
                ProcedureCount = procedureCount,
                MedicalRecordCount = medicalRecordCount,
                Revenue = revenue,
                RecentAppointments = recent,
                TodayAppointments = todayAppointments,
                TodayDate = today.ToString("D")
            };
            return View(vm);
        }


        public async Task<IActionResult> Doctors(string search, int page = 1)
        {
            const int pageSize = 15;
            page = Paging.Normalize(page);
            var query = _context.Doctors.Include(d => d.User).AsQueryable();

            List<Doctor>? matches = null;
            if (CiSearch.HasQuery(search))
            {
                var needle = CiSearch.Normalize(search);
                matches = (await query.ToListAsync())
                    .Where(d => CiSearch.Normalize(d.User.FirstName).Contains(needle)
                             || CiSearch.Normalize(d.User.LastName).Contains(needle)
                             || CiSearch.Normalize(d.Specialization).Contains(needle))
                    .OrderByDescending(d => d.Id)
                    .ToList();
            }

            var total = matches?.Count ?? await query.CountAsync();
            var list = matches != null
                ? matches.Skip((page - 1) * pageSize).Take(pageSize).ToList()
                : await query.OrderByDescending(d => d.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new DoctorsListViewModel
            {
                Doctors = list,
                CurrentPage = page,
                TotalPages = Paging.TotalPages(total, pageSize),
                Search = search ?? string.Empty
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDoctor(string firstName, string lastName, string phone, string email, string specialization, int yearsOfExperience)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(specialization))
            {
                TempData["Error"] = "Заполните все поля для врача";
                return RedirectToAction("Doctors");
            }
            if (await _context.Users.AnyAsync(u => u.Username != null && u.Phone == phone.Trim()))
            {
                TempData["Error"] = "Пользователь с таким телефоном уже существует";
                return RedirectToAction("Doctors");
            }

            var login = await _authService.GenerateUniqueUsernameAsync(firstName, lastName);
            var password = AppAuthService.GenerateRandomPassword(10);
            var user = new User { Username = login, Password = password, FirstName = firstName.Trim(), LastName = lastName.Trim(), Phone = phone.Trim(), Email = email.Trim(), Role = UserRole.Doctor, IsActive = true };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var doctor = new Doctor { UserId = user.Id, Specialization = specialization.Trim(), YearsOfExperience = yearsOfExperience, Bio = string.Empty };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Добавление врача", $"{firstName} {lastName}, логин: {login}");
            TempData["Success"] = $"Врач добавлен. Логин: {login}, Пароль: {password}";
            return RedirectToAction("Doctors");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDoctor(int id, string firstName, string lastName, string phone, string email, string specialization, int yearsOfExperience)
        {
            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();
            if (doctor.User != null)
            {
                doctor.User.FirstName = firstName.Trim();
                doctor.User.LastName = lastName.Trim();
                doctor.User.Phone = phone.Trim();
                doctor.User.Email = email.Trim();
            }
            doctor.Specialization = specialization.Trim();
            doctor.YearsOfExperience = yearsOfExperience;
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Обновление врача", $"{firstName} {lastName}");
            TempData["Success"] = "Данные врача обновлены";
            return RedirectToAction("Doctors");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();

            var hasAppointments = await _context.Appointments.AnyAsync(a => a.DoctorId == id);
            if (hasAppointments)
            {
                var docName = $"{doctor.User?.LastName} {doctor.User?.FirstName}".Trim();
                TempData["Error"] = $"Нельзя удалить врача «{docName}»: за ним числятся приёмы. Сначала отмените или перенесите их.";
                return RedirectToAction("Doctors");
            }

            var name = $"{doctor.User?.FirstName} {doctor.User?.LastName}".Trim();
            if (doctor.User != null) _context.Users.Remove(doctor.User);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Удаление врача", name);
            TempData["Success"] = "Врач удалён";
            return RedirectToAction("Doctors");
        }


        public async Task<IActionResult> Procedures(string search, int page = 1)
        {
            const int pageSize = 15;
            page = Paging.Normalize(page);
            var query = _context.Procedures.AsQueryable();

            List<Procedure>? matches = null;
            if (CiSearch.HasQuery(search))
            {
                var needle = CiSearch.Normalize(search);
                matches = (await query.ToListAsync())
                    .Where(p => CiSearch.Normalize(p.Name).Contains(needle)
                             || CiSearch.Normalize(p.Description).Contains(needle))
                    .OrderByDescending(p => p.Id)
                    .ToList();
            }

            var total = matches?.Count ?? await query.CountAsync();
            var list = matches != null
                ? matches.Skip((page - 1) * pageSize).Take(pageSize).ToList()
                : await query.OrderByDescending(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new ProceduresListViewModel
            {
                Procedures = list,
                CurrentPage = page,
                TotalPages = Paging.TotalPages(total, pageSize),
                Search = search ?? string.Empty
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProcedure(string name, string description, decimal price, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description) || price <= 0)
            {
                TempData["Error"] = "Заполните поля процедуры и укажите цену > 0";
                return RedirectToAction("Procedures");
            }
            _context.Procedures.Add(new Procedure { Name = name.Trim(), Description = description.Trim(), Price = price, DurationMinutes = durationMinutes > 0 ? durationMinutes : 30 });
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Добавление процедуры", name);
            TempData["Success"] = "Процедура добавлена";
            return RedirectToAction("Procedures");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProcedure(int id, string name, string description, decimal price, int durationMinutes)
        {
            var proc = await _context.Procedures.FindAsync(id);
            if (proc == null) return NotFound();
            proc.Name = name.Trim();
            proc.Description = description.Trim();
            proc.Price = price;
            proc.DurationMinutes = durationMinutes > 0 ? durationMinutes : 30;
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Обновление процедуры", name);
            TempData["Success"] = "Процедура обновлена";
            return RedirectToAction("Procedures");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProcedure(int id)
        {
            var proc = await _context.Procedures.FindAsync(id);
            if (proc == null) return NotFound();

            var hasAppointments = await _context.Appointments.AnyAsync(a => a.ProcedureId == id);
            if (hasAppointments)
            {
                TempData["Error"] = $"Нельзя удалить процедуру «{proc.Name}»: она назначена в существующих приёмах.";
                return RedirectToAction("Procedures");
            }

            var name = proc.Name;
            _context.Procedures.Remove(proc);
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Удаление процедуры", name);
            TempData["Success"] = "Процедура удалена";
            return RedirectToAction("Procedures");
        }


        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderBy(u => u.Role).ThenBy(u => u.LastName)
                .ToListAsync();

            var vm = new UsersListViewModel
            {
                Users = users.Select(u => new UsersListViewModel.UserRow
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = $"{u.LastName} {u.FirstName}",
                    Role = u.Role,
                    Phone = u.Phone,
                    LastLogin = u.LastLogin,
                    IsActive = u.IsActive,
                    CanReset = u.Role != UserRole.Admin
                }).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.Role == UserRole.Admin)
            {
                TempData["Error"] = "Сброс пароля администратора недоступен — используйте смену пароля в профиле";
                return RedirectToAction("Users");
            }

            var newPassword = AppAuthService.GenerateRandomPassword(10);
            user.Password = newPassword;
            await _context.SaveChangesAsync();
            await _audit.LogAsync("Сброс пароля", $"Пользователь: {user.Username} ({user.FirstName} {user.LastName})");
            TempData["Success"] = $"Новый пароль для {user.Username}: {newPassword}";
            return RedirectToAction("Users");
        }
    }
}
