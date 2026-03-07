using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Backend.Models;

namespace Backend.Controllers
{
  [Route("patients")]
  [Produces("application/json")]
  [Tags("Patient")]
  [ApiController]
  public class PatientController : BaseApiController
  {
    private readonly DataContext _dataContext;
    protected readonly AuthService _authService;

    public PatientController(DataContext dataContext, AuthService authService)
    {
      _dataContext = dataContext;
      _authService = authService;
    }

    /// <summary>
    /// Retrieves a paged list of active patients (Admin Only).
    /// </summary>
    /// <remarks>
    /// **Administrative Use:**
    /// - Filters out patients marked as `IsDeleted`.
    /// - Supports tokenized search by Firstname and Lastname.
    /// - Results are sorted by Lastname, then Firstname.
    /// </remarks>
    /// <param name="q">Search term for name filtering.</param>
    /// <param name="page">The page number (starts at 1).</param>
    /// <param name="pageSize">Items per page (max 100).</param>
    /// <response code="200">Returns a paged list of patient details.</response>
    /// <response code="401">Unauthorized: Missing or invalid Admin JWT.</response>
    /// <response code="403">Forbidden: User lacks the Admin role.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResponseDTO<PatientDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPatients(
    [FromQuery] string? q,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 100
    )
    {
      page = Math.Max(page, 1);
      pageSize = Math.Clamp(pageSize, 1, 100);

      var query = _dataContext.Patients.AsNoTracking().AsQueryable();

      if (!string.IsNullOrWhiteSpace(q))
      {
        var tokens = q.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLower())
            .ToArray();
        foreach (var token in tokens)
        {
          query = query.Where(d =>
              d.Firstname.ToLower().Contains(token) ||
              d.Lastname.ToLower().Contains(token));
        }
      }

      var total = await query.CountAsync();
      var data = await query
          .OrderBy(p => p.Lastname)
          .ThenBy(p => p.Firstname)
          .ThenBy(p => p.Id)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .Where(p => !p.IsDeleted)
          .Select(p => new PatientDetailsDto
          {
            Id = p.Id,
            Firstname = p.Firstname,
            Lastname = p.Lastname,
            Email = p.Email,
            DateOfBirth = p.DateOfBirth,
            IsGuest = p.IsGuest
          })
          .ToListAsync();

      return Ok(new PagedResponseDTO<PatientDetailsDto>
      {
        Data = data,
        Pagination = new PaginationDTO
        {
          Page = page,
          PageSize = pageSize,
          Total = total,
          TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        }
      });
    }

    /// <summary>
    /// Retrieves a specific patient profile by ID.
    /// </summary>
    /// <remarks>
    /// **Access Control:**
    /// - Patients can only view their own profile (ID must match JWT 'sub').
    /// - Admins can view any patient profile.
    /// </remarks>
    /// <response code="200">Returns the requested patient profile.</response>
    /// <response code="401">Unauthorized: Valid JWT is required.</response>
    /// <response code="403">Forbidden: You cannot access profiles other than your own.</response>
    /// <response code="404">Not Found: Patient ID does not exist.</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PatientProfileDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientProfileDTO>> GetPatient(Guid id)
    {
      var sub = User.FindFirst("sub")?.Value;
      var isAdmin = User.IsInRole("Admin");
      if (!isAdmin && id.ToString() != sub)
      {
        return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDTO
        {
          StatusCode = 403,
          Message = "You are not authorized to view another patient's profile."
        });
      }

      var patient = await _dataContext.Patients
          .Where(p => p.Id == id)
          .Select(p => new PatientProfileDTO
          {
            Id = p.Id,
            Firstname = p.Firstname,
            Lastname = p.Lastname,
            Email = p.Email,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            Address = p.Address,
            IsGuest = p.IsGuest,
            Religion = p.Religion,
            DriverLicenseNumber = p.DriverLicenseNumber,
            MedicalInsuranceMemberNumber = p.MedicalInsuranceMemberNumber,
            TaxNumber = p.TaxNumber,
            SocialSecurityNumber = p.SocialSecurityNumber,
          })
          .FirstOrDefaultAsync();
      if (patient is null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Patient with id {id} was not found."
        });
      }

      if (isAdmin)
      {
        return Ok(new PatientDetailsDto
        {
          Id = patient.Id,
          Firstname = patient.Firstname,
          Lastname = patient.Lastname,
          Email = patient.Email,
          DateOfBirth = patient.DateOfBirth,
          IsGuest = patient.IsGuest,
        });
      }

      return Ok(patient);
    }

    /// <summary>
    /// Creates a new patient profile (Admin Only).
    /// </summary>
    /// <remarks>
    /// Creates a patient record without credentials. For self-service registration use /auth/register.
    /// </remarks>
    /// <response code="201">Created: Returns created patient details.</response>
    /// <response code="400">Bad Request: Validation failed.</response>
    /// <response code="401">Unauthorized: Missing/invalid JWT.</response>
    /// <response code="403">Forbidden: Admin role required.</response>
    /// <response code="409">Conflict: Email already in use.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PatientDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePatient(
      [FromBody] CreatePatientAdminDto dto,
      [FromServices] IValidator<CreatePatientAdminDto> validator)
    {
      var validationResult = await validator.ValidateAsync(dto);
      if (!validationResult.IsValid) return ValidationBadRequest(validationResult);

      var firstname = dto.Firstname.Trim();
      var lastname = dto.Lastname.Trim();

      string? email = null;
      if (!string.IsNullOrWhiteSpace(dto.Email))
        email = dto.Email.Trim().ToLowerInvariant();

      if (email != null)
      {
        var exists = await _dataContext.Patients
          .AnyAsync(p => p.Email != null && p.Email.ToLower() == email);

        if (exists)
          return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Email already in use." });
      }

      var entity = new Patient
      {
        Id = Guid.NewGuid(),
        Firstname = firstname,
        Lastname = lastname,
        Email = email,
        DateOfBirth = dto.DateOfBirth,
        IsGuest = true,
        IsDeleted = false,
      };

      _dataContext.Patients.Add(entity);
      await _dataContext.SaveChangesAsync();

      var result = new PatientDetailsDto
      {
        Id = entity.Id,
        Firstname = entity.Firstname,
        Lastname = entity.Lastname,
        Email = entity.Email,
        DateOfBirth = entity.DateOfBirth.Value,
        IsGuest = entity.IsGuest,
      };

      return CreatedAtAction(nameof(GetPatient), new { Id = entity.Id }, result);
    }

    /// <summary>
    /// Partially updates a patient's profile.
    /// </summary>
    /// <remarks>
    /// **Partial Update:** Only provide fields that need changing.
    /// **Access Control:** Patients can only update themselves. Admins can update anyone.
    /// </remarks>
    /// <response code="200">Success: Returns the full updated profile.</response>
    /// <response code="400">Bad Request: Validation failed.</response>
    /// <response code="401">Unauthorized: Valid JWT is required.</response>
    /// <response code="403">Forbidden: Attempted to update another user's profile.</response>
    /// <response code="409">Conflict: Email is already in use by another account.</response>
    [HttpPatch("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PatientProfileDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePatient(
    Guid id,
    [FromBody] UpdatePatientDto dto,
    [FromServices] IValidator<UpdatePatientDto> validator)
    {
      var isAdmin = User.IsInRole("Admin");
      var sub = User.FindFirst("sub")?.Value;

      if (!isAdmin && id.ToString() != sub)
      {
        return StatusCode(403, new ApiErrorDTO { StatusCode = 403, Message = "You can only update your own profile." });
      }

      var validationResult = await validator.ValidateAsync(dto);
      if (!validationResult.IsValid) return ValidationBadRequest(validationResult);

      var entity = await _dataContext.Patients.FindAsync(id);
      if (entity == null || entity.IsDeleted)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = "Patient not found." });

      if (dto.Firstname != null) entity.Firstname = dto.Firstname.Trim();
      if (dto.Lastname != null) entity.Lastname = dto.Lastname.Trim();

      if (dto.Email != null)
      {
        var newEmail = dto.Email.Trim().ToLowerInvariant();
        if (newEmail != entity.Email.ToLowerInvariant())
        {
          var exists = await _dataContext.Patients.AnyAsync(p => p.Email.ToLower() == newEmail && p.Id != id);
          if (exists) return Conflict(new ApiErrorDTO { Message = "Email already in use." });
          entity.Email = newEmail;
        }
      }

      if (dto.DateOfBirth.HasValue) entity.DateOfBirth = dto.DateOfBirth;
      if (dto.Gender != null) entity.Gender = dto.Gender.Trim();
      if (dto.Address != null) entity.Address = dto.Address.Trim();
      if (dto.Religion != null) entity.Religion = dto.Religion.Trim();
      if (dto.DriverLicenseNumber != null) entity.DriverLicenseNumber = dto.DriverLicenseNumber.Trim();
      if (dto.TaxNumber != null) entity.TaxNumber = dto.TaxNumber.Trim();
      if (dto.SocialSecurityNumber != null) entity.SocialSecurityNumber = dto.SocialSecurityNumber.Trim();

      try
      {
        await _dataContext.SaveChangesAsync();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed due to database constraint." });
      }

      return Ok(new PatientProfileDTO
      {
        Id = entity.Id,
        Firstname = entity.Firstname,
        Lastname = entity.Lastname,
        Email = entity.Email,
        DateOfBirth = entity.DateOfBirth,
        Gender = entity.Gender,
        Address = entity.Address,
        IsGuest = entity.IsGuest,
        Religion = entity.Religion,
        DriverLicenseNumber = entity.DriverLicenseNumber,
        MedicalInsuranceMemberNumber = entity.MedicalInsuranceMemberNumber,
        TaxNumber = entity.TaxNumber,
        SocialSecurityNumber = entity.SocialSecurityNumber
      });
    }

    /// <summary>
    /// Securely updates the authenticated user's password.
    /// </summary>
    /// <remarks>
    /// **Security Requirements:**
    /// - Requires the `OldPassword` to prevent unauthorized password changes.
    /// - Only registered Patients can change passwords (Guest accounts are blocked).
    /// </remarks>
    /// <response code="204">Success: Password has been updated.</response>
    /// <response code="400">Bad Request: Validation failed or incorrect current password.</response>
    /// <response code="401">Unauthorized: Missing valid JWT.</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
  [FromBody] ChangePasswordDTO dto,
  [FromServices] IValidator<ChangePasswordDTO> validator
  )
    {
      var validationResult = await validator.ValidateAsync(dto);
      if (!validationResult.IsValid) return ValidationBadRequest(validationResult);

      var sub = User.FindFirst("sub")?.Value;
      if (sub == null) return Unauthorized();
      var patient = await _dataContext.Patients.FindAsync(Guid.Parse(sub));

      if (patient == null || patient.IsDeleted)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = "Patient not found." });

      if (patient.IsGuest)
        return BadRequest(new ApiErrorDTO { StatusCode = 400, Message = "Guest accounts do not have passwords. Please register first." });

      bool isValid = await _authService.ValidateUserAsync(patient, dto.OldPassword);
      if (!isValid)
        return BadRequest(new ApiErrorDTO { StatusCode = 400, Message = "Current password is incorrect." });

      var passwordHasher = new PasswordHasher<Patient>();
      patient.PasswordHash = passwordHasher.HashPassword(patient, dto.NewPassword);

      await _dataContext.SaveChangesAsync();

      return NoContent();
    }

    /// <summary>
    /// Anonymizes a patient's profile (Soft Delete).
    /// </summary>
    /// <remarks>
    /// **Privacy Compliance:** 
    /// Overwrites all PII with null or generic placeholders while retaining the record 
    /// to maintain historical appointment database integrity.
    /// </remarks>
    /// <response code="204">Success: Record anonymized.</response>
    /// <response code="403">Forbidden: You cannot anonymize other users.</response>
    /// <response code="404">Not Found: Patient ID invalid.</response>
    [HttpDelete("anonymize/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnonymizePatient(Guid id)
    {
      var isAdmin = User.IsInRole("Admin");
      var sub = User.FindFirst("sub")?.Value;

      if (!isAdmin && id.ToString() != sub)
      {
        return StatusCode(403, new ApiErrorDTO { Message = "You are not authorized to anonymize this profile." });
      }

      var entity = await _dataContext.Patients.FindAsync(id);
      if (entity == null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = "Patient was not found."
        });
      }
      if (entity.IsDeleted) return NoContent();

      var hasUpcomingAppointments = await _dataContext.Appointments.AnyAsync(a => a.PatientId == id && a.StartAt > DateTime.UtcNow);

      if (hasUpcomingAppointments)
      {
        return Conflict(new ApiErrorDTO
        {
          StatusCode = 409,
          Message = "Patient has upcoming appointments. Cancel them before anonymizing the profile."
        });
      }

      entity.Firstname = "[deleted]";
      entity.Lastname = "[deleted]";
      entity.Email = null;
      entity.DateOfBirth = null;
      entity.IsDeleted = true;
      entity.IsGuest = true;
      entity.PasswordHash = null;
      entity.Gender = null;
      entity.Religion = null;
      entity.Address = null;
      entity.DriverLicenseNumber = null;
      entity.MedicalInsuranceMemberNumber = null;
      entity.TaxNumber = null;
      entity.SocialSecurityNumber = null;

      await _dataContext.SaveChangesAsync();
      return NoContent();
    }

    /// <summary>
    /// Permanently deletes a patient record.
    /// </summary>
    /// <remarks>
    /// **Constraint:** 
    /// Hard deletion is only allowed if the patient has NO historical appointments. 
    /// For active patients, use the `anonymize` endpoint instead.
    /// </remarks>
    /// <response code="204">Success: Record permanently removed.</response>
    /// <response code="401">Unauthorized: Valid JWT required.</response>
    /// <response code="403">Forbidden: Insufficient permissions.</response>
    /// <response code="409">Conflict: Patient has existing appointments and cannot be removed.</response>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeletePatient(Guid id)
    {
      var isAdmin = User.IsInRole("Admin");
      var sub = User.FindFirst("sub")?.Value;

      if (!isAdmin && id.ToString() != sub)
      {
        return StatusCode(403, new ApiErrorDTO { Message = "You are not authorized to delete this profile." });
      }

      var patient = await _dataContext.Patients.FindAsync(id);
      if (patient == null) return NotFound();

      var hasAppointments = await _dataContext.Appointments.AnyAsync(a => a.PatientId == id);
      if (hasAppointments)
      {
        return Conflict(new ApiErrorDTO
        {
          StatusCode = 409,
          Message = "Cannot delete patient with existing appointments. Please use anonymize instead."
        });
      }

      _dataContext.Patients.Remove(patient);
      await _dataContext.SaveChangesAsync();
      return NoContent();
    }
  }
}