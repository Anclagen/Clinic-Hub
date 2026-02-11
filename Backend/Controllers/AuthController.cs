using Microsoft.AspNetCore.Mvc;
namespace Backend.Controllers
{
  [ApiController]
  [Route("auth")]
  public class AuthController : ControllerBase
  {
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
      _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "username": "neo",
    ///   "password": "matrix"
    /// }
    /// </remarks>
    /// <response code="200">Returns a JWT token.</response>
    /// <response code="401">Invalid username or password.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO request)
    {
      var user = await _authService.GetUserByEmailAsync(request.Email);

      if (user == null)
        return Unauthorized("Invalid email or password");
      if (user.IsDeleted == true)
        return Unauthorized("This user has been deleted");
      if (user.PasswordHash == null)
        return Unauthorized("You must register first");

      if (!await _authService.ValidateUserAsync(request.Email, request.Password))
        return Unauthorized("Invalid email or password");

      var token = _authService.GenerateToken(user);
      return Ok(new LoginResponseDto
      {
        Token = token,
        Id = user.Id,
        Email = user.Email,
        Firstname = user.Firstname,
        Lastname = user.Lastname,
        DateOfBirth = user.DateOfBirth,
      });
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "username": "neo",
    ///   "password": "matrix"
    /// }
    /// </remarks>
    /// <response code="200">User successfully registered.</response>
    /// <response code="400">Email already exists.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO request)   //change this to use DTO instead
    {
      if (await _authService.RegisterUserAsync(request.Email, request.Password, request.Firstname, request.Lastname, request.DateOfBirth))
      {
        return Ok("Registration successful");
      }
      return BadRequest("Email already exists");
    }
  }
}