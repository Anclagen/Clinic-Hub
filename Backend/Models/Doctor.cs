namespace Backend.Models

{
  public class Doctor
  {
    public Guid Id { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public int SpecialityId { get; set; }
    public required Speciality Speciality { get; set; }
    public int ClinicId { get; set; }
    public required Clinic Clinic { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
  }
}