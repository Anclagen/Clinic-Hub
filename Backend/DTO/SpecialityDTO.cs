public class CreateSpecialityDTO
{
  public required string SpecialityName { get; set; }
  public string? Description { get; set; }
}

public class UpdateSpecialityDTO
{
  public string? SpecialityName { get; set; }
  public string? Description { get; set; }

}

public class SpecialityResponseDTO
{
  public int Id { get; set; }
  public required string SpecialityName { get; set; }
  public string? Description { get; set; }
}