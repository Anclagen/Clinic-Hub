using ZstdSharp.Unsafe;

public record DoctorResponseDTO
{
  public Guid Id { get; set; }
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public string? ImageUrl { get; set; }
  public required int SpecialityId { get; set; }
  public required string SpecialityName { get; set; }
  public required int ClinicId { get; set; }
  public required string ClinicName { get; set; }
}

public record DoctorWithAppointmentsDTO
{
  public Guid Id { get; set; }
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public required int SpecialityId { get; set; }
  public required string SpecialityName { get; set; }
  public required int ClinicId { get; set; }
  public required string ClinicName { get; set; }
  public List<AppointmentResponseDTO> Appointments { get; set; } = [];
}


public record CreateDoctorDTO
{
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public string? ImageUrl { get; set; }
  public required int SpecialityId { get; set; }
  public required int ClinicId { get; set; }
}

public record UpdateDoctorDTO
{
  public string? Firstname { get; set; }
  public string? Lastname { get; set; }
  public string? ImageUrl { get; set; }
  public int? SpecialityId { get; set; }
  public int? ClinicId { get; set; }
}

public record DoctorQueryDTO
{
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 100;
  public int? SpecialityId { get; set; }
  public int? ClinicId { get; set; }
}

public record DoctorSearchQueryDTO
{
  public string? Query { get; init; }
  public int Page { get; init; } = 1;
  public int PageSize { get; init; } = 100;
  public int? SpecialityId { get; set; }
  public int? ClinicId { get; set; }
}