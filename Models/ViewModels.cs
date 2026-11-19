namespace MedicalCenterWeb.Models
{

    public class AdminDashboardViewModel
    {
        public int DoctorCount { get; set; }
        public int PatientCount { get; set; }
        public int AppointmentCount { get; set; }
        public int ProcedureCount { get; set; }
        public int MedicalRecordCount { get; set; }
        public decimal Revenue { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new();
        public List<Appointment> TodayAppointments { get; set; } = new();
        public string TodayDate { get; set; } = string.Empty;
    }

    public class StaffDashboardViewModel
    {
        public int PatientCount { get; set; }
        public int AppointmentCount { get; set; }
        public List<Appointment> TodayAppointments { get; set; } = new();
        public string TodayDate { get; set; } = string.Empty;
    }

    public class DoctorsListViewModel
    {
        public List<Doctor> Doctors { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string Search { get; set; } = string.Empty;
    }

    public class ProceduresListViewModel
    {
        public List<Procedure> Procedures { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string Search { get; set; } = string.Empty;
    }

    public class UsersListViewModel
    {
        public List<UserRow> Users { get; set; } = new();

        public class UserRow
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public UserRole Role { get; set; }
            public string Phone { get; set; } = string.Empty;
            public DateTime? LastLogin { get; set; }
            public bool IsActive { get; set; }
            public bool CanReset { get; set; }
        }
    }

    public class PatientsListViewModel
    {
        public List<Patient> Patients { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string Search { get; set; } = string.Empty;
    }

    public class AppointmentsPageViewModel
    {
        public List<Appointment> Appointments { get; set; } = new();
        public List<Patient> Patients { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
        public List<Procedure> Procedures { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string Search { get; set; } = string.Empty;
        public int? StatusFilter { get; set; }
        public int? DoctorFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class DoctorAppointmentsViewModel
    {
        public Doctor Doctor { get; set; } = null!;
        public List<Appointment> Appointments { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string Search { get; set; } = string.Empty;
    }

    public class PatientHistoryViewModel
    {
        public Patient Patient { get; set; } = null!;
        public List<Appointment> Appointments { get; set; } = new();
    }

    public class MedicalRecordEditViewModel
    {
        public Appointment Appointment { get; set; } = null!;
        public MedicalRecord? Record { get; set; }
    }

    public class ScheduleIndexViewModel
    {
        public List<Doctor> Doctors { get; set; } = new();
    }

    public class AuditLogListViewModel
    {
        public List<AuditLog> Logs { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int Total { get; set; }
        public string Search { get; set; } = string.Empty;
    }

    public class StatsIndexViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int ScheduledAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DateCountItem> WeekAppointments { get; set; } = new();
        public List<DoctorStatItem> TopDoctors { get; set; } = new();
        public List<ProcedureStatItem> PopularProcedures { get; set; } = new();
    }


    public class DateCountItem
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DoctorStatItem
    {
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ProcedureStatItem
    {
        public string ProcedureName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
