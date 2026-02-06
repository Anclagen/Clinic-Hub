public class CreateAppointmentDTO
{
  public Guid PatientId { get; set; }
  public int ClinicId { get; set; }
  public Guid DoctorId { get; set; }
  public int CategoryId { get; set; }
  public int DurationMinutes { get; set; }
  public DateTime StartAt { get; set; }
}

public class AppointmentResponseDTO
{
  public Guid PatientId { get; set; }
  public int ClinicId { get; set; }
  public string ClinicName { get; set; }
  public Guid DoctorId { get; set; }

  public int CategoryId { get; set; }
  public int DurationMinutes { get; set; }
  public DateTime StartAt { get; set; }
}