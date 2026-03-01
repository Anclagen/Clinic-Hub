using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using FluentValidation;

namespace Backend.Controllers
{
  [Route("clinics")]
  [Produces("application/json")]
  [Tags("Clinic")]
  [ApiController]
  public class ClinicController : BaseApiController
  {
    private readonly DataContext _dataContext;

    public ClinicController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    /// <summary>
    /// Retrieves all clinics.
    /// </summary>
    /// <returns>A list of clinics</returns>
    /// <response code="200">Success: Returns the clinic</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<ClinicResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClinics([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
      page = Math.Max(page, 1);
      pageSize = Math.Clamp(pageSize, 1, 100);

      var query = _dataContext.Clinics.AsNoTracking();
      var total = await query.CountAsync();

      var data = await query
          .OrderBy(c => c.ClinicName)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .Select(c => new ClinicResponseDTO
          {
            Id = c.Id,
            ClinicName = c.ClinicName,
            Address = c.Address,
            ImageUrl = c.ImageUrl,
            ImageAlt = c.ImageAlt
          })
          .ToListAsync();

      return Ok(new PagedResponseDTO<ClinicResponseDTO>
      {
        Data = data,
        Pagination = new PaginationDTO { Page = page, PageSize = pageSize, Total = total, TotalPages = (int)Math.Ceiling(total / (double)pageSize) }
      });
    }

    /// <summary>
    /// Retrieves a clinic by ID.
    /// </summary>
    /// <param name="id">The clinic ID.</param>
    /// <response code="200">Success: Returns the clinic</response>
    /// <response code="404">Not Found: If the clinic is not found</response>
    /// <response code="500">Internal Server Error: Something went wrong server side.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClinicResponseDTO), 200)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<ActionResult<ClinicResponseDTO>> GetClinic(int id)
    {
      var clinic = await _dataContext.Clinics
          .Where(c => c.Id == id)
          .Select(c => new ClinicResponseDTO
          {
            Id = c.Id,
            ClinicName = c.ClinicName,
            Address = c.Address,
            ImageUrl = c.ImageUrl,
            ImageAlt = c.ImageAlt,
            Doctors = c.Doctors
              .OrderBy(d => d.Lastname)
              .ThenBy(d => d.Firstname)
              .Select(d => new ClinicDoctorOptionDTO
              {
                Id = d.Id,
                Firstname = d.Firstname,
                Lastname = d.Lastname,
                ImageUrl = d.ImageUrl,
                SpecialityId = d.SpecialityId,
                SpecialityName = d.Speciality.SpecialityName
              })
              .ToList()
          })
          .FirstOrDefaultAsync();
      if (clinic is null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Clinic with id {id} was not found."
        });
      }
      return Ok(clinic);
    }

    /// <summary>
    /// Creates a new clinic into the system (Admin Only).
    /// </summary>
    /// <remarks>
    /// **Administrative Task:**
    /// Creates a new physical location where doctors can be assigned. The clinic name must be unique.
    /// </remarks>
    /// <param name="dto">The details of the clinic to be created.</param>
    /// <param name="validator">Injected FluentValidator for CreateClinicDTO.</param>
    /// <response code="201">Success: Returns the newly created clinic object.</response>
    /// <response code="400">Bad Request: Validation failed (e.g., name is missing or URL is malformed).</response>
    /// <response code="401">Unauthorized: JWT token is missing or invalid.</response>
    /// <response code="403">Forbidden: Authenticated user lacks 'Admin' permissions.</response>
    /// <response code="409">Conflict: A clinic with this name already exists.</response>
    /// <response code="500">Internal Server Error: A server-side error occurred while saving.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ClinicResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddClinic(
            [FromBody] CreateClinicDTO dto,
            [FromServices] IValidator<CreateClinicDTO> validator)
    {
      var result = await validator.ValidateAsync(dto);
      if (!result.IsValid) return ValidationBadRequest(result);

      var normalized = dto.ClinicName.Trim().ToLower();
      if (await _dataContext.Clinics.AnyAsync(c => c.ClinicName.ToLower() == normalized))
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = $"Clinic '{dto.ClinicName}' already exists." });

      var entity = new Clinic
      {
        ClinicName = dto.ClinicName.Trim(),
        Address = dto.Address,
        ImageUrl = dto.ImageUrl,
        ImageAlt = dto.ImageAlt
      };

      _dataContext.Clinics.Add(entity);
      await _dataContext.SaveChangesAsync();

      return CreatedAtAction(nameof(GetClinic), new { id = entity.Id }, new ClinicResponseDTO
      {
        Id = entity.Id,
        ClinicName = entity.ClinicName,
        Address = entity.Address,
        ImageUrl = entity.ImageUrl,
        ImageAlt = entity.ImageAlt
      });
    }

    /// <summary>
    /// Partially updates an existing clinic profile (Admin Only).
    /// </summary>
    /// <remarks>
    /// **Partial Update Behavior:**
    /// Only provide the fields you wish to change. Null or omitted fields will retain their current values.
    /// **Authorization:**
    /// Requires a JWT with the 'Admin' role claim.
    /// </remarks>
    /// <param name="id">The unique integer ID of the clinic.</param>
    /// <param name="dto">The fields to be updated.</param>
    /// <param name="validator">Injected FluentValidator for UpdateClinicDTO.</param>
    /// <response code="200">Success: Returns the full updated clinic object.</response>
    /// <response code="400">Bad Request: Validation failed (e.g., malformed URL or empty name).</response>
    /// <response code="401">Unauthorized: JWT token is missing or invalid.</response>
    /// <response code="403">Forbidden: Authenticated user lacks 'Admin' permissions.</response>
    /// <response code="404">Not Found: No clinic found with the provided ID.</response>
    /// <response code="409">Conflict: The new clinic name is already in use by another location.</response>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ClinicResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateClinic(
                int id,
                [FromBody] UpdateClinicDTO dto,
                [FromServices] IValidator<UpdateClinicDTO> validator)
    {
      var validationResult = await validator.ValidateAsync(dto);
      if (!validationResult.IsValid) return ValidationBadRequest(validationResult);

      var entity = await _dataContext.Clinics.FindAsync(id);
      if (entity == null) return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Clinic {id} not found." });

      if (!string.IsNullOrWhiteSpace(dto.ClinicName))
      {
        var normalized = dto.ClinicName.Trim().ToLower();
        if (await _dataContext.Clinics.AnyAsync(c => c.Id != id && c.ClinicName.ToLower() == normalized))
          return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Another clinic with this name exists." });

        entity.ClinicName = dto.ClinicName.Trim();
      }

      if (dto.Address != null) entity.Address = dto.Address;
      if (dto.ImageUrl != null) entity.ImageUrl = dto.ImageUrl;
      if (dto.ImageAlt != null) entity.ImageAlt = dto.ImageAlt;

      await _dataContext.SaveChangesAsync();

      return Ok(new ClinicResponseDTO
      {
        Id = entity.Id,
        ClinicName = entity.ClinicName,
        Address = entity.Address,
        ImageUrl = entity.ImageUrl,
        ImageAlt = entity.ImageAlt
      });
    }

    /// <summary>
    /// Deletes a clinic (Admin Only).
    /// </summary>
    /// <remarks>
    /// **Safety Guard:**
    /// Deletion is blocked if any doctors or appointments are still linked to this clinic.
    /// </remarks>
    /// <param name="id">The unique ID of the clinic to remove.</param>
    /// <response code="204">Success: Clinic deleted.</response>
    /// <response code="401">Unauthorized: Admin JWT required.</response>
    /// <response code="403">Forbidden: Insufficient permissions.</response>
    /// <response code="404">Not Found: Clinic ID does not exist.</response>
    /// <response code="409">Conflict: Clinic is referenced by doctors or appointments.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Sharpened: Added Admin Role
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteClinic(int id)
    {
      var clinic = await _dataContext.Clinics.FindAsync(id);
      if (clinic is null) return NotFound();

      var hasDoctors = await _dataContext.Doctors.AnyAsync(d => d.ClinicId == id);
      if (hasDoctors)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete clinic while doctors are still assigned to it." });

      var hasAppointments = await _dataContext.Appointments.AnyAsync(d => d.ClinicId == id);
      if (hasAppointments)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete clinic with appointments." });

      _dataContext.Clinics.Remove(clinic);
      await _dataContext.SaveChangesAsync();

      return NoContent();
    }
  }
}
