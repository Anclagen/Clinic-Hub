public class CreateClinicDTO
{
  public required string ClinicName { get; set; }
  public string? Address { get; set; }
  public string? ImageUrl { get; set; }
  public string? ImageAlt { get; set; }
}

public class UpdateClinicDTO
{
  public string? ClinicName { get; set; }
  public string? Address { get; set; }
  public string? ImageUrl { get; set; }
  public string? ImageAlt { get; set; }

}

public class ClinicResponseDTO
{
  public int Id { get; set; }
  public required string ClinicName { get; set; }
  public string? Address { get; set; }
  public string? ImageUrl { get; set; }
  public string? ImageAlt { get; set; }
  public List<ClinicDoctorOptionDTO> Doctors { get; set; } = [];
}

public class ClinicDoctorOptionDTO
{
  public Guid Id { get; set; }
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public string? ImageUrl { get; set; }
  public int SpecialityId { get; set; }
  public required string SpecialityName { get; set; }
}
