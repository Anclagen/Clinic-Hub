namespace Backend.Models

{
  public class Doctor
  {
    public Guid Id { get; set; }
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public int SpecialityId { get; set; }
    public Speciality Speciality { get; set; } = null!;
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
  }
}