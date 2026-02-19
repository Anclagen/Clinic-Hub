public class CreateAppointmentDTO
{

  public Guid? PatientId { get; set; }
  public string? Firstname { get; set; }
  public string? Lastname { get; set; }
  public DateOnly? DateOfBirth { get; set; }
  public int ClinicId { get; set; }
  public Guid DoctorId { get; set; }
  public int CategoryId { get; set; }
  public int DurationMinutes { get; set; }
  public DateTime StartAt { get; set; }
}

public class AppointmentResponseDTO
{
  public Guid Id { get; set; }

  public string Firstname { get; set; }
  public string Lastname { get; set; }

  public DateOnly? DateOfBirth { get; set; }

  public Guid PatientId { get; set; }
  public int ClinicId { get; set; }
  public string ClinicName { get; set; }
  public Guid DoctorId { get; set; }
  public string DoctorName { get; set; }
  public int CategoryId { get; set; }
  public string CategoryName { get; set; }


  public int Duration { get; set; }
  public DateTime StartAt { get; set; }
}

public class BookedTimeSlotDTO
{
  public DateTime StartAt { get; set; }
  public DateTime EndAt { get; set; }
}
