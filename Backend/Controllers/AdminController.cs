using Backend.Data;
using Backend.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
  [Route("admins")]
  [Produces("application/json")]
  [Tags("Admin")]
  [ApiController]
  [Authorize(Roles = "Admin")]
  public class AdminController : BaseApiController
  {
    private readonly DataContext _dataContext;

    public AdminController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    private static IQueryable<AdminResponseDTO> MapAdminToResponse(IQueryable<Admin> query)
    {
      return query.Select(a => new AdminResponseDTO
      {
        Id = a.Id,
        Username = a.Username,
        Email = a.Email
      });
    }

    /// <summary>
    /// Lists all administrative accounts in the system.(Admin Only)
    /// </summary>
    /// <remarks>
    /// **Authorization:** Restricted to users with the 'Admin' role.
    /// 
    /// **Behavior:** Returns a list of all administrators, sorted alphabetically by username. Sensitive data like `PasswordHash` is excluded from the response.
    /// </remarks>
    /// <response code="200">Success: Returns a list of administrator profiles.</response>
    /// <response code="401">Unauthorized: Missing or invalid JWT token.</response>
    /// <response code="403">Forbidden: User is authenticated but lacks the 'Admin' role.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AdminResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdmins()
    {
      var admins = await MapAdminToResponse(_dataContext.Admins.AsNoTracking())
        .OrderBy(a => a.Username)
        .ToListAsync();

      return Ok(admins);
    }

    /// <summary>
    /// Retrieves a specific administrator by their unique ID.(Admin only)
    /// </summary>
    /// <param name="id">The GUID of the administrator.</param>
    /// <response code="200">Success: Returns the requested admin profile.</response>
    /// <response code="404">Not Found: No administrator exists with the provided ID.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AdminResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdmin(Guid id)
    {
      var admin = await MapAdminToResponse(_dataContext.Admins.AsNoTracking())
        .FirstOrDefaultAsync(a => a.Id == id);

      if (admin is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = "Admin not found." });

      return Ok(admin);
    }

    /// <summary>
    /// Creates a new administrative account.(Admin only)
    /// </summary>
    /// <remarks>
    /// **Security Logic:**
    /// - Passwords are salted and hashed using the `Identity.PasswordHasher`.
    /// - Usernames are trimmed and Emails are normalized to lowercase.
    /// 
    /// **Validation:**
    /// - Email and Username must be unique.
    /// </remarks>
    /// <param name="dto">The account details including a plaintext password.</param>
    /// <response code="201">Created: The new admin was successfully registered.</response>
    /// <response code="400">Bad Request: Validation failed (e.g., weak password, malformed email).</response>
    /// <response code="409">Conflict: An account with this username or email already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AdminResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAdmin(
      [FromBody] CreateAdminDTO dto,
      [FromServices] IValidator<CreateAdminDTO> validator)
    {
      var result = await validator.ValidateAsync(dto);
      if (!result.IsValid) return ValidationBadRequest(result);

      var entity = new Admin
      {
        Username = dto.Username.Trim(),
        Email = dto.Email.Trim().ToLowerInvariant(),
        PasswordHash = string.Empty
      };

      var hasher = new PasswordHasher<Admin>();
      entity.PasswordHash = hasher.HashPassword(entity, dto.Password);

      _dataContext.Admins.Add(entity);

      try
      {
        await _dataContext.SaveChangesAsync();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Admin with the same username or email already exists." });
      }

      return CreatedAtAction(nameof(GetAdmin), new { id = entity.Id }, new AdminResponseDTO
      {
        Id = entity.Id,
        Username = entity.Username,
        Email = entity.Email
      });
    }

    /// <summary>
    /// Updates an administrator's profile or password.(Admin only)
    /// </summary>
    /// <remarks>
    /// **Partial Updates Supported:**
    /// Only the fields provided in the request body will be updated.
    /// 
    /// **Normalization:**
    /// If Username or Email is updated, it is checked for uniqueness across all other accounts.
    /// </remarks>
    /// <response code="200">Success: Returns the updated admin profile.</response>
    /// <response code="409">Conflict: The new username or email is already taken.</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(AdminResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAdmin(
      Guid id,
      [FromBody] UpdateAdminDTO dto,
      [FromServices] IValidator<UpdateAdminDTO> validator)
    {
      var result = await validator.ValidateAsync(dto);
      if (!result.IsValid) return ValidationBadRequest(result);

      var entity = await _dataContext.Admins.FindAsync(id);
      if (entity is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = "Admin not found." });

      if (dto.Username != null)
      {
        var normalizedUsername = dto.Username.Trim().ToLower();
        var usernameExists = await _dataContext.Admins.AnyAsync(a => a.Id != id && a.Username.ToLower() == normalizedUsername);
        if (usernameExists)
          return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Username already exists." });

        entity.Username = dto.Username.Trim();
      }

      if (dto.Email != null)
      {
        var normalizedEmail = dto.Email.Trim().ToLower();
        var emailExists = await _dataContext.Admins.AnyAsync(a => a.Id != id && a.Email.ToLower() == normalizedEmail);
        if (emailExists)
          return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Email already exists." });

        entity.Email = dto.Email.Trim().ToLowerInvariant();
      }

      if (dto.Password != null)
      {
        var hasher = new PasswordHasher<Admin>();
        entity.PasswordHash = hasher.HashPassword(entity, dto.Password);
      }

      try
      {
        await _dataContext.SaveChangesAsync();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed due to a database conflict." });
      }

      return Ok(new AdminResponseDTO
      {
        Id = entity.Id,
        Username = entity.Username,
        Email = entity.Email
      });
    }

    /// <summary>
    /// Permanently removes an administrative account.(Admin only)
    /// </summary>
    /// <remarks>
    /// **Fail-Safe Mechanism:**
    /// To prevent accidental total lockout, the system will **deny** deletion if this is the last remaining administrator account.
    /// </remarks>
    /// <param name="id">The GUID of the admin to delete.</param>
    /// <response code="204">No Content: The admin was successfully deleted.</response>
    /// <response code="409">Conflict: Deletion denied because this is the last admin in the system.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAdmin(Guid id)
    {
      var admin = await _dataContext.Admins.FindAsync(id);
      if (admin is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = "Admin not found." });

      var adminCount = await _dataContext.Admins.CountAsync();
      if (adminCount <= 1)
      {
        return Conflict(new ApiErrorDTO
        {
          StatusCode = 409,
          Message = "Cannot delete the last remaining admin."
        });
      }

      _dataContext.Admins.Remove(admin);
      await _dataContext.SaveChangesAsync();

      return NoContent();
    }
  }
}
