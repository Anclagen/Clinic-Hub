using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using FluentValidation;

namespace Backend.Controllers
{
  [Route("specialities")]
  [Produces("application/json")]
  [Tags("Speciality")]
  [ApiController]
  public class SpecialityController : BaseApiController
  {
    private readonly DataContext _dataContext;

    public SpecialityController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    /// <summary>
    /// Retrieves a paged list of all available medical specialities.
    /// </summary>
    /// <param name="page">The page number (defaults to 1).</param>
    /// <param name="pageSize">Items per page (max 100, defaults to 20).</param>
    /// <response code="200">Returns a paged wrapper of specialities.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<SpecialityResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSpecialities(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
      page = Math.Max(page, 1);
      pageSize = Math.Clamp(pageSize, 1, 100);

      var query = _dataContext.Specialities.AsNoTracking();
      var total = await query.CountAsync();

      var data = await query
          .OrderBy(s => s.SpecialityName)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .Select(s => new SpecialityResponseDTO
          {
            Id = s.Id,
            SpecialityName = s.SpecialityName,
            Description = s.Description,
          })
          .ToListAsync();

      return Ok(new PagedResponseDTO<SpecialityResponseDTO>
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
    /// Retrieves a specific speciality by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SpecialityResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpecialityResponseDTO>> GetSpeciality(int id)
    {
      var speciality = await _dataContext.Specialities
          .AsNoTracking()
          .Where(s => s.Id == id)
          .Select(s => new SpecialityResponseDTO
          {
            Id = s.Id,
            SpecialityName = s.SpecialityName,
            Description = s.Description,
          })
          .FirstOrDefaultAsync();

      if (speciality is null)
        return NotFound(new ApiErrorDTO { Message = $"Speciality {id} not found." });

      return Ok(speciality);
    }

    /// <summary>
    /// Creates a new medical speciality (Admin Only).
    /// </summary>
    /// <response code="201">Success: Speciality created.</response>
    /// <response code="400">Bad Request: If the speciality name is null</response>
    /// <response code="401">Unauthorized: Admin JWT required.</response>
    /// <response code="403">Forbidden: Admin JWT required.</response>
    /// <response code="409">Conflict: Speciality name already exists.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SpecialityResponseDTO), 201)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddSpeciality(
            [FromBody] CreateSpecialityDTO dto,
            [FromServices] IValidator<CreateSpecialityDTO> validator)
    {
      var result = await validator.ValidateAsync(dto);
      if (!result.IsValid) return ValidationBadRequest(result);

      var normalized = dto.SpecialityName.Trim().ToLower();
      if (await _dataContext.Specialities.AnyAsync(s => s.SpecialityName.ToLower() == normalized))
        return Conflict(new ApiErrorDTO { Message = $"Speciality '{dto.SpecialityName}' already exists." });

      var entity = new Speciality
      {
        SpecialityName = dto.SpecialityName.Trim(),
        Description = dto.Description,
      };

      _dataContext.Specialities.Add(entity);
      await _dataContext.SaveChangesAsync();

      var response = new SpecialityResponseDTO
      {
        Id = entity.Id,
        SpecialityName = entity.SpecialityName,
        Description = entity.Description
      };

      return CreatedAtAction(nameof(GetSpeciality), new { id = response.Id }, response);
    }

    /// <summary>
    /// Updates a speciality by its ID.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "specialityName": "Exsanguinater",
    ///   "description": "Skilled at removing all the patients blood"
    /// }
    /// </remarks>
    /// <param name="id">Speciality ID</param>
    /// <param name="dto">Request Body</param>
    /// <param name="validator">Fluent Validator injected in to validate body</param>
    /// <response code="204">Confirms update with status code.</response>
    /// <response code="400">Bad Request: Validation error</response>
    /// <response code="401">Unauthorised: If you lack an jwt token in your request headers</response>
    /// <response code="403">Forbidden: Admin JWT required.</response>
    /// <response code="404">Not Found: If the speciality can't be found</response>
    /// <response code="409">Conflict: If the speciality name already exists</response>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SpecialityResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateSpeciality(
        int id,
        [FromBody] UpdateSpecialityDTO dto,
        [FromServices] IValidator<UpdateSpecialityDTO> validator)
    {
      var validationResult = await validator.ValidateAsync(dto);
      if (!validationResult.IsValid) return ValidationBadRequest(validationResult);

      var entity = await _dataContext.Specialities.FindAsync(id);
      if (entity == null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Speciality {id} not found." });

      if (!string.IsNullOrWhiteSpace(dto.SpecialityName))
      {
        var trimmedName = dto.SpecialityName.Trim();
        var normalized = trimmedName.ToLower();

        var exists = await _dataContext.Specialities
            .AnyAsync(s => s.Id != id && s.SpecialityName.ToLower() == normalized);

        if (exists)
          return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Another speciality with this name exists." });

        entity.SpecialityName = trimmedName;
      }

      if (dto.Description != null)
        entity.Description = dto.Description.Trim();

      await _dataContext.SaveChangesAsync();

      var response = new SpecialityResponseDTO
      {
        Id = entity.Id,
        SpecialityName = entity.SpecialityName,
        Description = entity.Description
      };

      return Ok(response);
    }

    /// <summary>
    /// Deletes a speciality
    /// </summary>
    /// <param name="id">Speciality ID</param>
    /// <response code="204">Confirms deletion with status code.</response>
    /// <response code="401">Unauthorised: If you lack an jwt token in your request headers</response>
    /// <response code="403">Forbidden: Admin JWT required.</response>
    /// <response code="404">Not Found: If the speciality can't be found</response>
    /// <response code="409">Conflict: If the speciality is currently linked to a doctor</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteSpeciality(int id)
    {
      var speciality = await _dataContext.Specialities.FindAsync(id);
      if (speciality is null) return NotFound();

      var hasDoctors = await _dataContext.Doctors.AnyAsync(d => d.SpecialityId == id);
      if (hasDoctors)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete speciality: doctors are currently assigned to it." });

      _dataContext.Specialities.Remove(speciality);
      await _dataContext.SaveChangesAsync();

      return NoContent();
    }
  }
}