public class ApiErrorDTO
{
  /// <example>400</example>
  public int StatusCode { get; set; }
  /// <example>Bad Request</example>
  public string Message { get; set; } = "";
  /// <example>TeamId with id 666 was not found</example>
  public string? Detail { get; set; }
  /// <example>TeamId</example>
  public string? Field { get; set; }
}

public class ApiNotFoundErrorDTO
{
  /// <example>404</example>
  public int StatusCode { get; set; }
  /// <example>Entity with id 666 was not found</example>
  public string Message { get; set; } = "";
}