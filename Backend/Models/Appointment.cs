namespace Backend.Models
{
  public class Appointment
  {
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }
    public required Patient Patient { get; set; }
    public int ClinicId { get; set; }
    public required Clinic Clinic { get; set; }
    public Guid DoctorId { get; set; }
    public required Doctor Doctor { get; set; }
    public int CategoryId { get; set; }
    public required Category Category { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime StartAt { get; set; }
  }
}