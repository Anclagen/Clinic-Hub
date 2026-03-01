using Microsoft.AspNetCore.Mvc;
using FluentValidation;
namespace Backend.Controllers
{
  [ApiController]
  [Route("auth")]
  public class AuthController : BaseApiController
  {
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
      _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <response code="200">Returns a JWT token and user profile.</response>
    /// <response code="401">Invalid credentials or account deactivated.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(typeof(ApiErrorDTO), 401)]
    public async Task<IActionResult> Login([FromBody] LoginDTO request)
    {
      var normalizedEmail = request.Email.Trim().ToLowerInvariant();
      var user = await _authService.GetUserByEmailAsync(normalizedEmail);

      if (user == null || user.IsDeleted || user.PasswordHash == null)
        return Unauthorized(new ApiErrorDTO { StatusCode = 401, Message = "Invalid email or password" });

      if (!await _authService.ValidateUserAsync(user, request.Password))
        return Unauthorized(new ApiErrorDTO { StatusCode = 401, Message = "Invalid email or password" });

      var token = _authService.GenerateToken(user);

      return Ok(new LoginResponseDto
      {
        Token = token,
        Id = user.Id,
        Email = user.Email,
        Firstname = user.Firstname,
        Lastname = user.Lastname,
        DateOfBirth = user.DateOfBirth
      });
    }

    /// <summary>
    /// Registers a new patient account.
    /// </summary>
    /// <remarks>
    /// **Note:** This creates a full patient profile. 
    /// If the user previously booked as a guest with the same details, 
    /// you may want to implement logic later to link those records.
    /// 
    /// Sample request:
    ///
    ///     POST /auth/register
    ///     {
    ///        "email": "patient@example.com",
    ///        "firstname": "Thomas",
    ///        "lastname": "Anderson",
    ///        "password": "SecurePassword123!",
    ///        "dateOfBirth": "1999-01-01"
    ///     }
    /// </remarks>
    /// <response code="200">Success: Returns a confirmation message.</response>
    /// <response code="400">Bad Request: Email is already in use or validation failed.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
      [FromBody] RegisterDTO request,
      [FromServices] IValidator<RegisterDTO> validator)
    {
      var validationResult = await validator.ValidateAsync(request);
      if (!validationResult.IsValid) return ValidationBadRequest(validationResult);

      if (await _authService.RegisterUserAsync(request.Email, request.Password, request.Firstname, request.Lastname, request.DateOfBirth))
      {
        return Ok("Registration successful");
      }

      return BadRequest(new ApiErrorDTO { StatusCode = 400, Message = "Email already exists" });
    }

    /// <summary>
    /// Authenticates administrative staff.
    /// </summary>
    /// <remarks>
    /// **Administrative Access:**
    /// Provides a token with the 'Admin' role claim. 
    /// Use this token for clinic, doctor, and category management.
    /// </remarks>
    /// <response code="200">Returns the JWT token and admin username.</response>
    /// <response code="401">Unauthorized: Invalid username or password.</response>
    [HttpPost("admin/login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // You could create an AdminLoginResponseDTO here too
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginDTO request)
    {

      var admin = await _authService.GetAdminUserByUsernameAsync(request.Username);

      if (admin == null || !await _authService.ValidateAdminUserAsync(admin, request.Password))
        return Unauthorized(new ApiErrorDTO { StatusCode = 401, Message = "Invalid username or password" });

      var token = _authService.GenerateAdminToken(admin);

      return Ok(new { Token = token, Username = admin.Username });
    }
  }
}