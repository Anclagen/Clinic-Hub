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

public sealed class AppointmentQueryDTO
{
  public int? ClinicId { get; init; }
  public Guid? DoctorId { get; init; }
  public Guid? PatientId { get; init; }

  public DateTime? From { get; init; }
  public DateTime? To { get; init; }

  public string? Status { get; init; }

  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 20;

  public string SortBy { get; init; } = "startAt";
  public string SortDir { get; init; } = "asc"; // "asc" | "desc"
}