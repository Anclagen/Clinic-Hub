using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace Backend.Controllers
{
  [Route("clinics")]
  [Produces("application/json")]
  [Tags("Clinic")]
  [ApiController]
  public class ClinicController : ControllerBase
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
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClinicResponseDTO>), 200)]
    public async Task<ActionResult<IEnumerable<ClinicResponseDTO>>> GetClinics()
    {
      var clinics = await _dataContext.Clinics
          .Select(c => new ClinicResponseDTO
          {
            Id = c.Id,
            ClinicName = c.ClinicName,
            Address = c.Address,
            ImageUrl = c.ImageUrl,
            ImageAlt = c.ImageAlt
          })
          .ToListAsync();

      return Ok(clinics);
    }

    /// <summary>
    /// Retrieves a clinic by ID.
    /// </summary>
    /// <param name="Id">The clinic ID.</param>
    /// <response code="200">Returns the clinic</response>
    /// <response code="404">If the clinic is not found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet("{Id}")]
    [ProducesResponseType(typeof(ClinicResponseDTO), 200)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<ActionResult<ClinicResponseDTO>> GetClinic(int Id)
    {
      var clinic = await _dataContext.Clinics
          .Where(c => c.Id == Id)
          .Select(c => new ClinicResponseDTO
          {
            Id = c.Id,
            ClinicName = c.ClinicName,
            Address = c.Address,
            ImageUrl = c.ImageUrl,
            ImageAlt = c.ImageAlt
          })
          .FirstOrDefaultAsync();
      if (clinic is null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Clinic with id {Id} was not found."
        });
      }
      return Ok(clinic);
    }

    /// <summary>
    /// Creates a clinic
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "clinicName": "Exsanguination",
    ///   "defaultDuration": 15,
    ///   "description": "Removal of all patients blood to offer to the vampiric overlords."
    /// }
    /// </remarks>
    /// <response code="201">Returns the newly created clinic</response>
    /// <response code="400">If the clinic name is null</response>
    /// <response code="409">If the clinic name already exists</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ClinicResponseDTO), 201)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClinicResponseDTO>> AddClinic([FromBody] CreateClinicDTO dto)
    {
      var clinicName = dto.ClinicName.Trim();
      var normalized = clinicName.ToLower();

      var nameExists = await _dataContext.Clinics.AnyAsync(c =>
          c.ClinicName.ToLower() == normalized);

      if (nameExists)
      {
        return Conflict(new ApiErrorDTO
        {
          StatusCode = 409,
          Message = $"Clinic with name '{dto.ClinicName}' already exists."
        });
      }

      var entity = new Clinic
      {
        ClinicName = clinicName,
        Address = dto.Address,
        ImageUrl = dto.ImageUrl,
        ImageAlt = dto.ImageAlt
      };

      _dataContext.Clinics.Add(entity);
      try { await _dataContext.SaveChangesAsync(); }
      catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Clinic name already exists." }); }

      var response = new ClinicResponseDTO
      {
        Id = entity.Id,
        ClinicName = entity.ClinicName,
        Address = entity.Address,
        ImageUrl = entity.ImageUrl,
        ImageAlt = entity.ImageAlt
      };

      return CreatedAtAction(nameof(GetClinic), new { Id = response.Id }, response);
    }

    /// <summary>
    /// Updates a clinic by its ID.
    /// </summary>
    /// <param name="Id">Clinic ID</param>
    /// <response code="204">Confirms update with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the clinic can't be found</response>
    /// <response code="409">If the clinic name already exists</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPut("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    [ProducesResponseType(typeof(ApiErrorDTO), 409)]
    public async Task<IActionResult> UpdateClinic(int Id, UpdateClinicDTO dto)
    {
      var entity = await _dataContext.Clinics.FindAsync(Id);
      if (entity == null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Clinic with id {Id} was not found."
        });
      }

      if (!string.IsNullOrWhiteSpace(dto.ClinicName) &&
        !string.Equals(dto.ClinicName, entity.ClinicName, StringComparison.OrdinalIgnoreCase))
      {
        var clinicName = dto.ClinicName.Trim();
        var normalized = clinicName.ToLower();

        var nameExists = await _dataContext.Clinics.AnyAsync(c =>
            c.ClinicName.ToLower() == normalized);

        if (nameExists)
        {
          return Conflict(new ApiErrorDTO
          {
            StatusCode = 409,
            Message = $"Clinic with name '{dto.ClinicName}' already exists."
          });
        }

        entity.ClinicName = clinicName;
      }
      if (dto.Address is not null)
        entity.Address = dto.Address;
      if (dto.ImageUrl is not null)
        entity.ImageUrl = dto.ImageUrl;
      if (dto.ImageAlt is not null)
        entity.ImageAlt = dto.ImageAlt;

      try { await _dataContext.SaveChangesAsync(); }
      catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Clinic name already exists." }); }

      return NoContent();
    }

    /// <summary>
    /// Deletes a clinic
    /// </summary>
    /// <param name="Id">Clinic ID</param>
    /// <response code="204">Confirms deletion with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the clinic can't be found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpDelete("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<IActionResult> DeleteClinic(int Id)
    {
      var clinic = await _dataContext.Clinics.FindAsync(Id);
      if (clinic is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Clinic with id {Id} was not found." });

      _dataContext.Clinics.Remove(clinic);

      try
      {
        await _dataContext.SaveChangesAsync();
        return NoContent();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete clinic because it is referenced by other records." });
      }
    }
  }
}