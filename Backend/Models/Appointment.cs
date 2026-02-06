namespace Backend.Models
{
  public class Appointment
  {
    public Guid Id { get; set; }

    public required Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public required int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public required Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public required int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public required int DurationMinutes { get; set; }
    public required DateTime StartAt { get; set; }
  }
}