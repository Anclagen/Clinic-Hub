namespace Backend.Models
{
  public class Clinic
  {
    public int Id { get; set; }
    public required string ClinicName { get; set; }
    public string? Address { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageAlt { get; set; }
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();


  }
}