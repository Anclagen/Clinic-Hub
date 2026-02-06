using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace Backend.Controllers
{
  [Route("specialities")]
  [Produces("application/json")]
  [Tags("Speciality")]
  [ApiController]
  public class SpecialityController : ControllerBase
  {
    private readonly DataContext _dataContext;

    public SpecialityController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    /// <summary>
    /// Retrieves all specialities.
    /// </summary>
    /// <returns>A list of specialities</returns>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpecialityResponseDTO>), 200)]
    public async Task<ActionResult<IEnumerable<SpecialityResponseDTO>>> GetSpecialities()
    {
      var specialities = await _dataContext.Specialities
          .Select(s => new SpecialityResponseDTO
          {
            Id = s.Id,
            SpecialityName = s.SpecialityName,
            Description = s.Description,
          })
          .ToListAsync();

      return Ok(specialities);
    }

    /// <summary>
    /// Retrieves a speciality by ID.
    /// </summary>
    /// <param name="Id">The speciality ID.</param>
    /// <response code="200">Returns the speciality</response>
    /// <response code="404">If the speciality is not found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet("{Id}")]
    [ProducesResponseType(typeof(SpecialityResponseDTO), 200)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<ActionResult<SpecialityResponseDTO>> GetSpeciality(int Id)
    {
      var speciality = await _dataContext.Specialities
          .Where(s => s.Id == Id)
          .Select(s => new SpecialityResponseDTO
          {
            Id = s.Id,
            SpecialityName = s.SpecialityName,
            Description = s.Description,
          })
          .FirstOrDefaultAsync();
      if (speciality is null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Speciality with id {Id} was not found."
        });
      }
      return Ok(speciality);
    }

    /// <summary>
    /// Creates a speciality
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "specialityName": "Exsanguinater",
    ///   "description": "Skilled at removing all the patients blood"
    /// }
    /// </remarks>
    /// <response code="201">Returns the newly created speciality</response>
    /// <response code="400">If the speciality name is null</response>
    /// <response code="409">If the speciality name already exists</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(SpecialityResponseDTO), 201)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SpecialityResponseDTO>> AddSpeciality([FromBody] CreateSpecialityDTO dto)
    {
      var specialityName = dto.SpecialityName?.Trim();
      if (string.IsNullOrWhiteSpace(specialityName))
      {
        return BadRequest(new ApiErrorDTO
        {
          StatusCode = 400,
          Message = "SpecialityName is required."
        });
      }
      var normalized = specialityName.ToLower();

      var nameExists = await _dataContext.Specialities.AnyAsync(c =>
          c.SpecialityName.ToLower() == normalized);

      if (nameExists)
      {
        return Conflict(new ApiErrorDTO
        {
          StatusCode = 409,
          Message = $"Speciality with name '{dto.SpecialityName}' already exists."
        });
      }

      var entity = new Speciality
      {
        SpecialityName = specialityName,
        Description = dto.Description,

      };

      _dataContext.Specialities.Add(entity);
      try { await _dataContext.SaveChangesAsync(); }
      catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Create failed due to database constraint." }); }

      var response = new SpecialityResponseDTO
      {
        Id = entity.Id,
        SpecialityName = entity.SpecialityName,
        Description = entity.Description,
      };

      return CreatedAtAction(nameof(GetSpeciality), new { Id = response.Id }, response);
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
    /// <param name="Id">Speciality ID</param>
    /// <response code="204">Confirms update with status code.</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the speciality can't be found</response>
    /// <response code="409">If the speciality name already exists</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPut("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    [ProducesResponseType(typeof(ApiErrorDTO), 409)]
    public async Task<IActionResult> UpdateSpeciality(int Id, UpdateSpecialityDTO dto)
    {
      var entity = await _dataContext.Specialities.FindAsync(Id);
      if (entity == null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Speciality with id {Id} was not found."
        });
      }
      if (!string.IsNullOrWhiteSpace(dto.SpecialityName))
      {
        var specialityName = dto.SpecialityName.Trim();
        if (!string.Equals(specialityName, entity.SpecialityName, StringComparison.OrdinalIgnoreCase))
        {

          var normalized = specialityName.ToLower();

          var nameExists = await _dataContext.Specialities.AnyAsync(c =>
            c.Id != Id &&
            c.SpecialityName.ToLower() == normalized);

          if (nameExists)
          {
            return Conflict(new ApiErrorDTO
            {
              StatusCode = 409,
              Message = $"Speciality with name '{dto.SpecialityName}' already exists."
            });
          }

          entity.SpecialityName = specialityName;
        }
      }
      if (dto.Description is not null)
        entity.Description = dto.Description;

      try { await _dataContext.SaveChangesAsync(); }
      catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed due to database constraint." }); }

      return NoContent();
    }

    /// <summary>
    /// Deletes a speciality
    /// </summary>
    /// <param name="Id">Speciality ID</param>
    /// <response code="204">Confirms deletion with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the speciality can't be found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpDelete("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<IActionResult> DeleteSpeciality(int Id)
    {
      var speciality = await _dataContext.Specialities.FindAsync(Id);
      if (speciality is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Speciality with id {Id} was not found." });

      _dataContext.Specialities.Remove(speciality);

      try
      {
        await _dataContext.SaveChangesAsync();
        return NoContent();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete speciality because it is referenced by other records." });
      }
    }
  }
}