using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Data
{
  public class DataContext : DbContext
  {
    public DataContext(DbContextOptions options) : base(options)
    {

    }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Clinic> Clinics { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Speciality> Specialities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
       d => d.ToDateTime(TimeOnly.MinValue),
       d => DateOnly.FromDateTime(d)
     );

      modelBuilder.Entity<Patient>()
        .Property(p => p.DateOfBirth)
        .HasColumnType("date")
        .HasConversion(dateOnlyConverter);

      modelBuilder.Entity<Patient>()
        .HasIndex(p => p.Email)
        .IsUnique();

      modelBuilder.Entity<Category>()
        .HasIndex(c => c.CategoryName)
        .IsUnique();

      modelBuilder.Entity<Clinic>()
        .HasIndex(c => c.ClinicName)
        .IsUnique();

      modelBuilder.Entity<Speciality>()
        .HasIndex(s => s.SpecialityName)
        .IsUnique();

      modelBuilder.Entity<Doctor>()
        .HasOne(d => d.Speciality)
        .WithMany(s => s.Doctors)
        .HasForeignKey(d => d.SpecialityId)
        .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Doctor>()
        .HasOne(d => d.Clinic)
        .WithMany(c => c.Doctors)
        .HasForeignKey(d => d.ClinicId)
        .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Appointment>()
        .HasOne(a => a.Patient)
        .WithMany(p => p.Appointments)
        .HasForeignKey(a => a.PatientId)
        .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Appointment>()
        .HasOne(a => a.Doctor)
        .WithMany(d => d.Appointments)
        .HasForeignKey(a => a.DoctorId)
        .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Appointment>()
        .HasOne(a => a.Category)
        .WithMany(c => c.Appointments)
        .HasForeignKey(a => a.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Appointment>()
        .HasOne(a => a.Clinic)
        .WithMany(c => c.Appointments)
        .HasForeignKey(a => a.ClinicId)
        .OnDelete(DeleteBehavior.Restrict);
    }
  }
}