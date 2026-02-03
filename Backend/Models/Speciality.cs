namespace Backend.Models
{
  public class Speciality
  {
    public int Id { get; set; }
    public required string SpecialityName { get; set; }
    public string? Description { get; set; }
  }
}