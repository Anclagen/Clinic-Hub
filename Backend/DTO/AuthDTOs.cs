public class LoginDTO
{
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
}

public class RegisterDTO
{
  public required string Email { get; set; }

  public required string Firstname { get; set; }
  public required string Lastname { get; set; }
  public required string Password { get; set; }
  public required DateOnly DateOfBirth { get; set; }
}

public class LoginResponseDto
{
  public string Token { get; set; } = string.Empty;
  public Guid Id { get; set; }
  public string Email { get; set; } = string.Empty;
  public string Firstname { get; set; } = string.Empty;
  public string Lastname { get; set; } = string.Empty;
  public DateOnly DateOfBirth { get; set; }
}