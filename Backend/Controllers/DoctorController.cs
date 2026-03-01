using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using FluentValidation;
using Backend.Extensions;

namespace Backend.Controllers
{
  [Route("doctors")]
  [Produces("application/json")]
  [Tags("Doctor")]
  [ApiController]
  public class DoctorController : BaseApiController
  {
    private readonly DataContext _dataContext;

    public DoctorController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    private IQueryable<DoctorResponseDTO> MapDoctorToResponse(IQueryable<Doctor> query)
    {
      return query.Select(d => new DoctorResponseDTO
      {
        Id = d.Id,
        Firstname = d.Firstname,
        Lastname = d.Lastname,
        ImageUrl = d.ImageUrl,
        SpecialityId = d.SpecialityId,
        SpecialityName = d.Speciality.SpecialityName,
        ClinicId = d.ClinicId,
        ClinicName = d.Clinic.ClinicName
      });
    }

    /// <summary>
    /// Retrieves a paged list of doctors for the public directory.
    /// </summary>
    /// <remarks>
    /// **Browse Logic:**
    /// - If no filters are provided, returns all active doctors.
    /// - Filters are additive (AND logic). Providing both Clinic and Speciality narrows the result to doctors matching both.
    /// 
    /// **Pagination:**
    /// - Results are sorted alphabetically by Last Name, then First Name.
    /// - Default: Page 1, Size 20.
    /// </remarks>
    /// <param name="q">Pagination and Filter parameters (ClinicId, SpecialityId).</param>
    /// <response code="200">Returns a paged wrapper containing the doctor profiles.</response>
    /// <response code="400">Bad Request: Invalid pagination parameters or malformed IDs.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<DoctorResponseDTO>), 200)]
    [ProducesResponseType(typeof(ApiErrorDTO), 400)]
    public async Task<IActionResult> GetDoctors([FromQuery] DoctorQueryDTO q)
    {
      var pageSize = Math.Clamp(q.PageSize, 1, 100);
      var page = Math.Max(q.Page, 1);

      var query = _dataContext.Doctors.AsNoTracking();

      if (q.ClinicId.HasValue)
        query = query.Where(d => d.ClinicId == q.ClinicId);

      if (q.SpecialityId.HasValue)
        query = query.Where(d => d.SpecialityId == q.SpecialityId);

      var total = await query.CountAsync();

      var data = await MapDoctorToResponse(query)
          .OrderBy(d => d.Lastname)
          .ThenBy(d => d.Firstname)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();

      return Ok(new PagedResponseDTO<DoctorResponseDTO>
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
    /// Searches for doctors using name-based tokenization and optional filters.
    /// </summary>
    /// <remarks>
    /// **Search Algorithm:**
    /// - Splits the `Query` string by spaces into individual tokens.
    /// - Each token must exist in either the Firstname or Lastname (AND logic between tokens).
    /// - Example: "John Smith" matches "Johnathan Smith" but not "John Doe".
    /// 
    /// **Safety:**
    /// - Page size is internally clamped between 1 and 100.
    /// - Page number is forced to a minimum of 1.
    /// </remarks>
    /// <param name="q">Search criteria including the name query and filter IDs.</param>
    /// <response code="200">Returns a paged list of matching doctors.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResponseDTO<DoctorResponseDTO>), 200)]
    public async Task<IActionResult> SearchDoctors([FromQuery] DoctorSearchQueryDTO q)
    {
      var query = _dataContext.Doctors.AsNoTracking();

      var pageSize = Math.Clamp(q.PageSize, 1, 100);
      var page = Math.Max(q.Page, 1);

      if (q.ClinicId.HasValue) query = query.Where(d => d.ClinicId == q.ClinicId);
      if (q.SpecialityId.HasValue) query = query.Where(d => d.SpecialityId == q.SpecialityId);

      if (!string.IsNullOrWhiteSpace(q.Query))
      {
        var tokens = q.Query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
          query = query.Where(d => d.Firstname.ToLower().Contains(token) || d.Lastname.ToLower().Contains(token));
        }
      }

      var total = await query.CountAsync();
      var data = await MapDoctorToResponse(query)
          .OrderBy(d => d.Lastname)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();

      return Ok(new PagedResponseDTO<DoctorResponseDTO>
      {
        Data = data,
        Pagination = new PaginationDTO
        {
          Total = total,
          Page = page,
          PageSize = pageSize,
          TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        }
      });
    }

    /// <summary>
    /// Retrieves detailed profile information for a specific doctor.
    /// </summary>
    /// <param name="Id">The unique GUID of the doctor.</param>
    /// <response code="200">Returns the doctor's profile and associated clinic/speciality details.</response>
    /// <response code="404">Not Found: The specified doctor ID does not exist.</response>
    [HttpGet("{Id}")]
    [ProducesResponseType(typeof(DoctorResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorResponseDTO>> GetDoctor(Guid Id)
    {
      var doctor = await MapDoctorToResponse(_dataContext.Doctors.AsNoTracking())
          .FirstOrDefaultAsync(d => d.Id == Id);

      if (doctor is null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = "Doctor not found."
        });
      }

      return Ok(doctor);
    }

    /// <summary>
    /// Registers a new doctor into the system.
    /// </summary>
    /// <remarks>
    /// **Authorization:** Requires an 'Admin' role claim.
    /// 
    /// **Validation Rules:**
    /// - Firstname and Lastname are mandatory and trimmed.
    /// - `SpecialityId` and `ClinicId` must exist in the database.
    /// - Returns a `Location` header in the response pointing to the new resource.
    /// </remarks>
    /// <response code="201">Success: Returns the full doctor profile with mapped entity names.</response>
    /// <response code="400">Bad Request: Validation failed (check error fields for details).</response>
    /// <response code="401">Unauthorized: JWT token is missing or expired.</response>
    /// <response code="403">Forbidden: Authenticated user lacks 'Admin' permissions.</response>
    /// <response code="500">Internal Server Error: Database persistence failed.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DoctorResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DoctorResponseDTO>> AddDoctor(
            [FromBody] CreateDoctorDTO dto,
            [FromServices] IValidator<CreateDoctorDTO> validator)
    {
      var result = await validator.ValidateAsync(dto);
      if (!result.IsValid) return ValidationBadRequest(result);

      var entity = new Doctor
      {
        Firstname = dto.Firstname.Trim(),
        Lastname = dto.Lastname.Trim(),
        ImageUrl = dto.ImageUrl,
        SpecialityId = dto.SpecialityId,
        ClinicId = dto.ClinicId
      };

      _dataContext.Doctors.Add(entity);
      await _dataContext.SaveChangesAsync();

      var response = await MapDoctorToResponse(_dataContext.Doctors.AsNoTracking())
          .FirstOrDefaultAsync(d => d.Id == entity.Id);

      if (response == null) return StatusCode(500, "Error retrieving created record.");

      return CreatedAtAction(nameof(GetDoctor), new { Id = response.Id }, response);
    }

    /// <summary>
    /// Updates an existing doctor's profile and returns the updated record.
    /// </summary>
    /// <remarks>
    /// **Partial Updates:** Only provide the fields you wish to change.
    /// </remarks>
    /// <response code="200">Success: Returns the full updated doctor profile.</response>
    /// <response code="400">Bad Request: Validation failed.</response>
    /// <response code="404">Not Found: Doctor ID does not exist.</response>
    [HttpPatch("{Id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DoctorResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDoctor(
        Guid Id,
        [FromBody] UpdateDoctorDTO dto,
        [FromServices] IValidator<UpdateDoctorDTO> validator)
    {
      var validationResult = await validator.ValidateAsync(dto);
      if (!validationResult.IsValid) return ValidationBadRequest(validationResult);

      var entity = await _dataContext.Doctors.FindAsync(Id);
      if (entity == null)
      {
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = "Doctor not found." });
      }

      if (dto.ClinicId.HasValue && dto.ClinicId.Value != entity.ClinicId)
      {
        // Check for future appointments at the OLD clinic
        var hasFutureAppointments = await _dataContext.Appointments
            .AnyAsync(a => a.DoctorId == Id &&
                           a.ClinicId == entity.ClinicId &&
                           a.StartAt > DateTime.UtcNow);
        if (hasFutureAppointments)
        {
          return Conflict(new ApiErrorDTO
          {
            StatusCode = 409,
            Message = "Cannot change clinic. This doctor has upcoming appointments at their current location. Please reassign or cancel them first."
          });
        }
        entity.ClinicId = dto.ClinicId.Value;
      }

      if (dto.SpecialityId.HasValue) entity.SpecialityId = dto.SpecialityId.Value;
      if (dto.Firstname != null) entity.Firstname = dto.Firstname.Trim();
      if (dto.Lastname != null) entity.Lastname = dto.Lastname.Trim();
      if (dto.ImageUrl != null) entity.ImageUrl = dto.ImageUrl;

      try
      {
        await _dataContext.SaveChangesAsync();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed due to a database conflict." });
      }

      var response = await MapDoctorToResponse(_dataContext.Doctors.AsNoTracking())
            .FirstOrDefaultAsync(d => d.Id == entity.Id);

      return Ok(response);
    }

    /// <summary>
    /// Removes a doctor from the directory.
    /// </summary>
    /// <remarks>
    /// **Safety Check:** /// To preserve historical data, a doctor cannot be deleted if they are linked to any existing appointments. 
    /// If you need to "deactivate" a doctor with appointments, consider adding an 'IsActive' flag instead.
    /// </remarks>
    /// <param name="Id">The unique GUID of the doctor to remove.</param>
    /// <response code="204">Success: Doctor has been permanently deleted.</response>
    /// <response code="401">Unauthorized: Valid Admin JWT required.</response>
    /// <response code="404">Not Found: No doctor found with the provided ID.</response>
    /// <response code="409">Conflict: Doctor cannot be deleted due to existing appointment records.</response>
    [HttpDelete("{Id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteDoctor(Guid Id)
    {
      var exists = await _dataContext.Doctors.AnyAsync(d => d.Id == Id);
      if (!exists)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = "Doctor not found." });

      var hasAppointments = await _dataContext.Appointments.AnyAsync(a => a.DoctorId == Id);
      if (hasAppointments)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Deletion denied. This doctor has existing appointments." });

      _dataContext.Doctors.Remove(new Doctor { Id = Id });

      try
      {
        await _dataContext.SaveChangesAsync();
        return NoContent();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Doctor is referenced by other system records and cannot be deleted." });
      }
    }
  }
}