using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System.Security.Claims;

namespace Backend.Controllers
{
  [Route("patients")]
  [Produces("application/json")]
  [Tags("Patient")]
  [ApiController]
  public class PatientController : ControllerBase
  {
    private readonly DataContext _dataContext;

    public PatientController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    /// <summary>
    /// Retrieves all patients.
    /// </summary>
    /// <returns>A list of patients</returns>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResponseDTO<PatientDetailsDto>), 200)]
    public async Task<IActionResult> GetPatients(
    [FromQuery] string? q,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20
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

      query = query.Where(p => !p.IsDeleted && p.DateOfBirth != null);
      var total = await query.CountAsync();
      var data = await query
          .OrderBy(p => p.Lastname)
          .ThenBy(p => p.Firstname)
          .ThenBy(p => p.Id)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .Select(p => new PatientDetailsDto
          {
            Id = p.Id,
            Firstname = p.Firstname,
            Lastname = p.Lastname,
            Email = p.Email,
            DateOfBirth = p.DateOfBirth!.Value
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
    /// Retrieves a patient by ID.
    /// </summary>
    /// <param name="Id">The patient ID.</param>
    /// <response code="200">Returns the patient</response>
    /// <response code="404">If the patient is not found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet("{Id}")]
    [Authorize]
    [ProducesResponseType(typeof(PatientProfileDTO), 200)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<ActionResult<PatientProfileDTO>> GetPatient(Guid Id)
    {

      var role = User.FindFirstValue(ClaimTypes.Role);
      var sub = User.FindFirst("sub")?.Value;

      var patient = await _dataContext.Patients
          .Where(p => p.Id == Id)
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
          Message = $"Patient with id {Id} was not found."
        });
      }

      if (patient.Id.ToString() != sub && role != "Admin")
      {
        return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorDTO
        {
          StatusCode = 403,
          Message = "You are not authorized to view another patient's profile."
        });
      }

      return Ok(new { Data = patient, Role = role, Sub = sub });
    }

    /// <summary>
    /// Updates a patient by its ID.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "firstname": "Doc",
    ///   "lastname": "Tor",
    /// }
    /// </remarks>
    /// <param name="Id">Patient ID</param>
    /// <response code="204">Confirms update with status code.</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the patient can't be found</response>
    /// <response code="409">Update failed due to database constraint.</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPatch("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    [ProducesResponseType(typeof(ApiErrorDTO), 409)]
    public async Task<IActionResult> UpdatePatient(Guid Id, PatientProfileDTO dto)
    {
      var entity = await _dataContext.Patients.FindAsync(Id);
      if (entity == null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Patient with id {Id} was not found."
        });
      }

      if (dto.Firstname is not null)
        entity.Firstname = dto.Firstname.Trim();

      if (dto.Lastname is not null)
        entity.Lastname = dto.Lastname.Trim();

      if (dto.Email is not null)
      {
        var newEmail = dto.Email.Trim().ToLowerInvariant();
        var currentEmail = entity.Email.Trim().ToLowerInvariant();

        if (newEmail != currentEmail)
        {
          var exists = await _dataContext.Patients
              .AnyAsync(p => p.Email.ToLower() == newEmail && p.Id != entity.Id);

          if (exists)
            return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed, email already exists." });

          entity.Email = newEmail;
        }
      }

      if (dto.DateOfBirth is DateOnly dob)
        entity.DateOfBirth = dob;

      if (dto.Gender is not null)
        entity.Gender = dto.Gender.Trim();

      if (dto.Religion is not null)
        entity.Religion = dto.Religion.Trim();

      if (dto.Address is not null)
        entity.Address = dto.Address.Trim();

      if (dto.DriverLicenseNumber is not null)
        entity.DriverLicenseNumber = dto.DriverLicenseNumber.Trim();

      if (dto.MedicalInsuranceMemberNumber is not null)
        entity.MedicalInsuranceMemberNumber = dto.MedicalInsuranceMemberNumber.Trim();

      if (dto.TaxNumber is not null)
        entity.TaxNumber = dto.TaxNumber.Trim();

      if (dto.SocialSecurityNumber is not null)
        entity.SocialSecurityNumber = dto.SocialSecurityNumber.Trim();

      try { await _dataContext.SaveChangesAsync(); }
      catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed due to database constraint." }); }

      return NoContent();
    }

    /// <summary>
    /// Deletes a patient
    /// </summary>
    /// <param name="Id">Patient ID</param>
    /// <response code="204">Confirms deletion with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the patient can't be found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpDelete("anonymize/{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<IActionResult> AnonymizePatient(Guid Id)
    {
      var entity = await _dataContext.Patients.FindAsync(Id);
      if (entity == null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Patient with id {Id} was not found."
        });
      }

      entity.Firstname = "[deleted]";
      entity.Lastname = "[deleted]";
      entity.Email = null;
      entity.DateOfBirth = null;
      entity.IsGuest = true;
      entity.IsDeleted = true;
      entity.PasswordHash = null;
      entity.Gender = null;
      entity.Religion = null;
      entity.Address = null;
      entity.DriverLicenseNumber = null;
      entity.MedicalInsuranceMemberNumber = null;
      entity.TaxNumber = null;
      entity.SocialSecurityNumber = null;

      try { await _dataContext.SaveChangesAsync(); }
      catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed due to database constraint." }); }

      return NoContent();
    }

    /// <summary>
    /// Deletes a patient
    /// </summary>
    /// <param name="Id">Patient ID</param>
    /// <response code="204">Confirms deletion with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the patient can't be found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpDelete("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<IActionResult> DeletePatient(Guid Id)
    {
      var patient = await _dataContext.Patients.FindAsync(Id);
      if (patient == null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Patient with id {Id} was not found." });

      var hasAppointments = await _dataContext.Appointments.AnyAsync(a => a.PatientId == Id);

      if (hasAppointments)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete patient because they have appointments." });
      }

      _dataContext.Patients.Remove(patient);

      try
      {
        await _dataContext.SaveChangesAsync();
        return NoContent();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete patient because it is referenced by other records." });
      }
    }
  }
}