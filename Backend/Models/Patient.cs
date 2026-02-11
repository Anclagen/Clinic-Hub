namespace Backend.Models
{
  public class Patient
  {
    public Guid Id { get; set; }
    public required string Firstname { get; set; } = null!;
    public required string Lastname { get; set; } = null!;
    public string? Email { get; set; }
    public bool IsGuest { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateOnly? DateOfBirth { get; set; }
    public string? PasswordHash { get; set; }

    public string? Gender { get; set; }
    public string? Religion { get; set; }
    public string? Address { get; set; }
    public string? DriverLicenseNumber { get; set; }
    public string? MedicalInsuranceMemberNumber { get; set; }
    public string? TaxNumber { get; set; }
    public string? SocialSecurityNumber { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

  }
}