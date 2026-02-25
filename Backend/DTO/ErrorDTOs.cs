public class ApiBadRequestErrorDTO
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

public class ApiErrorDTO
{
  public int StatusCode { get; set; }
  public string Message { get; set; } = "";
  public string? Detail { get; set; }
  public string? Field { get; set; }

  // Only used for validation / multi-field errors
  public Dictionary<string, string[]>? Errors { get; set; }
}