namespace Backend.Models
{
  public class Category
  {
    public int Id { get; set; }
    public required string CategoryName { get; set; }

    public required int DefaultDuration { get; set; }

    public string? Description { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
  }
}