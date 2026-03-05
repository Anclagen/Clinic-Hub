public record AdminResponseDTO
{
  public Guid Id { get; set; }
  public required string Username { get; set; }
  public required string Email { get; set; }
}

public record CreateAdminDTO
{
  public required string Username { get; set; }
  public required string Email { get; set; }
  public required string Password { get; set; }
}

public record UpdateAdminDTO
{
  public string? Username { get; set; }
  public string? Email { get; set; }
  public string? Password { get; set; }
}
