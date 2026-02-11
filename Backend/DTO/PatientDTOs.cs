public record PatientListDto
{
  public Guid Id { get; set; }
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public string Email { get; set; }
  public bool IsGuest { get; set; }
};

public record PatientDetailsDto
{
  public Guid Id { get; set; }
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public string Email { get; set; }
  public bool IsGuest { get; set; }
  public DateOnly DateOfBirth { get; set; }
};

public record CreatePatientDto(
  string Firstname,
  string Lastname,
  string Email,
  bool IsGuest,
  DateOnly DateOfBirth,
  string? Password
);

public record UpdatePatientDto(
  string? Firstname,
  string? Lastname,
  string? Email,
  DateOnly? DateOfBirth
);

public class PatientProfileDTO
{
  public Guid Id { get; set; }
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public string? Email { get; set; }
  public bool? IsGuest { get; set; }
  public DateOnly? DateOfBirth { get; set; }
  public string? Gender { get; set; }
  public string? Address { get; set; }
  public string? Religion { get; set; }
  public string? DriverLicenseNumber { get; set; }
  public string? MedicalInsuranceMemberNumber { get; set; }
  public string? TaxNumber { get; set; }
  public string? SocialSecurityNumber { get; set; }
}