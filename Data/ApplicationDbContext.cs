using Microsoft.EntityFrameworkCore;
using MedicalCenterWeb.Models;

namespace MedicalCenterWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Procedure> Procedures { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DoctorSchedule>()
                .HasOne(ds => ds.Doctor).WithMany(d => d.Schedules).HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor).WithMany(d => d.Appointments).HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Procedure).WithMany(pr => pr.Appointments).HasForeignKey(a => a.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(mr => mr.Appointment).WithOne(a => a.MedicalRecord)
                .HasForeignKey<MedicalRecord>(mr => mr.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<Patient>().HasIndex(p => p.Phone).IsUnique();
        }
    }
}
