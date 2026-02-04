public class CreateCategoryDTO
{
  public required string CategoryName { get; set; }
  public required int DefaultDuration { get; set; }
  public string? Description { get; set; }
}

public class UpdateCategoryDTO
{
  public string? CategoryName { get; set; }
  public int? DefaultDuration { get; set; }
  public string? Description { get; set; }

}

public class CategoryResponseDTO
{
  public int Id { get; set; }
  public required string CategoryName { get; set; }

  public required int DefaultDuration { get; set; }

  public string? Description { get; set; }
}