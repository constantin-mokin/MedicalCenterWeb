using System.ComponentModel.DataAnnotations;

namespace MedicalCenterWeb.Models
{
    public enum UserRole { Admin = 1, Doctor = 2, Staff = 3 }
    public enum Gender { Male = 0, Female = 1 }
    public enum AppointmentStatus { Scheduled = 0, Completed = 1, Cancelled = 2 }

    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин: от 3 до 50 символов")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Пароль: от 4 до 100 символов")]
        public string Password { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, Phone, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
    }

    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Укажите специализацию")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Специализация: от 2 до 100 символов")]
        public string Specialization { get; set; } = string.Empty;

        [Range(0, 70, ErrorMessage = "Стаж: от 0 до 70 лет")]
        public int YearsOfExperience { get; set; }

        [StringLength(1000)]
        public string Bio { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;
        public virtual List<DoctorSchedule> Schedules { get; set; } = new();
        public virtual List<Appointment> Appointments { get; set; } = new();
    }

    public class Staff
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;
    }

    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите имя"), StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите фамилию"), StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите телефон"), Phone, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }

        [StringLength(2000)]
        public string MedicalHistory { get; set; } = string.Empty;

        public virtual List<Appointment> Appointments { get; set; } = new();
    }

    public class Procedure
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите название"), StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000, ErrorMessage = "Цена должна быть больше нуля")]
        public decimal Price { get; set; }

        [Range(5, 480, ErrorMessage = "Длительность: от 5 до 480 минут")]
        public int DurationMinutes { get; set; }

        public virtual List<Appointment> Appointments { get; set; } = new();
    }

    public class DoctorSchedule
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public virtual Doctor Doctor { get; set; } = null!;
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int ProcedureId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public AppointmentStatus Status { get; set; }

        public decimal? RecordedPrice { get; set; }

        public virtual Patient Patient { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Procedure Procedure { get; set; } = null!;
        public virtual MedicalRecord? MedicalRecord { get; set; }
    }

    public class MedicalRecord
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }

        [StringLength(1000)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(1000)]
        public string ChronicDiseases { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Recommendations { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = Services.AppClock.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual Appointment Appointment { get; set; } = null!;
    }

    public class AuditLog
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [StringLength(500)]
        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = Services.AppClock.Now;
    }
}
