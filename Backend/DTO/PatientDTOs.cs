public record PatientListDto
{
  public Guid Id { get; set; }
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public string? Email { get; set; }
  public bool? IsGuest { get; set; }
};

public record PatientDetailsDto
{
  public Guid Id { get; set; }
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public string? Email { get; set; }
  public bool IsGuest { get; set; }
  public DateOnly? DateOfBirth { get; set; }
};

public class PatientProfileDTO
{
  public Guid Id { get; set; }
  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public string? Email { get; set; }
  public bool IsGuest { get; set; }
  public DateOnly? DateOfBirth { get; set; }
  public string? Gender { get; set; }
  public string? Address { get; set; }
  public string? Religion { get; set; }
  public string? DriverLicenseNumber { get; set; }
  public string? MedicalInsuranceMemberNumber { get; set; }
  public string? TaxNumber { get; set; }
  public string? SocialSecurityNumber { get; set; }
}

public class CreatePatientAdminDto
{
  public string Firstname { get; set; } = default!;
  public string Lastname { get; set; } = default!;
  public string? Email { get; set; }
  public DateOnly? DateOfBirth { get; set; }
}


public class UpdatePatientDto
{
  public string? Firstname { get; set; }
  public string? Lastname { get; set; }
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

public class ChangePasswordDTO
{
  public required string NewPassword { get; set; }
  public required string OldPassword { get; set; }
}