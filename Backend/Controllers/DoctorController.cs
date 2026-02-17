using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace Backend.Controllers
{
  [Route("doctors")]
  [Produces("application/json")]
  [Tags("Doctor")]
  [ApiController]
  public class DoctorController : ControllerBase
  {
    private readonly DataContext _dataContext;

    public DoctorController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    /// <summary>
    /// Retrieves all doctors.
    /// </summary>
    /// <returns>A list of doctors</returns>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<DoctorResponseDTO>), 200)]
    public async Task<IActionResult> GetDoctors(
    [FromQuery] string? q,
    [FromQuery] int? clinicId,
    [FromQuery] int? specialityId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20
)
    {
      page = Math.Max(page, 1);
      pageSize = Math.Clamp(pageSize, 1, 100);

      var query = _dataContext.Doctors.AsNoTracking().AsQueryable();

      if (clinicId.HasValue)
        query = query.Where(d => d.ClinicId == clinicId.Value);

      if (specialityId.HasValue)
        query = query.Where(d => d.SpecialityId == specialityId.Value);

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
          .OrderBy(d => d.Lastname)
          .ThenBy(d => d.Firstname)
          .ThenBy(d => d.Id)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .Select(d => new DoctorResponseDTO
          {
            Id = d.Id,
            Firstname = d.Firstname,
            Lastname = d.Lastname,
            ImageUrl = d.ImageUrl,
            SpecialityId = d.SpecialityId,
            SpecialityName = d.Speciality.SpecialityName,
            ClinicId = d.ClinicId,
            ClinicName = d.Clinic.ClinicName
          })
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
    /// Retrieves a doctor by ID.
    /// </summary>
    /// <param name="Id">The doctor ID.</param>
    /// <response code="200">Returns the doctor</response>
    /// <response code="404">If the doctor is not found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet("{Id}")]
    [ProducesResponseType(typeof(DoctorResponseDTO), 200)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<ActionResult<DoctorResponseDTO>> GetDoctor(Guid Id)
    {
      var doctor = await _dataContext.Doctors
          .Where(d => d.Id == Id)
          .Select(d => new DoctorResponseDTO
          {
            Id = d.Id,
            Firstname = d.Firstname,
            Lastname = d.Lastname,
            ImageUrl = d.ImageUrl,
            SpecialityId = d.SpecialityId,
            SpecialityName = d.Speciality.SpecialityName,
            ClinicId = d.ClinicId,
            ClinicName = d.Clinic.ClinicName,
          })
          .FirstOrDefaultAsync();
      if (doctor is null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Doctor with id {Id} was not found."
        });
      }
      return Ok(doctor);
    }

    /// <summary>
    /// Creates a doctor
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "firstname": "Doc",
    ///   "lastname": "Tor",
    ///   "imageUrl": "https://www.example.com/example.jpg"
    ///   "specialityId": 1,
    ///   "clinicId": 2
    /// }
    /// </remarks>
    /// <response code="201">Returns the newly created doctor</response>
    /// <response code="400">If the fullname is null</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="409">Create failed due to database constraint.</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(DoctorResponseDTO), 201)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DoctorResponseDTO>> AddDoctor([FromBody] CreateDoctorDTO dto)
    {
      var lookup = await (
          from s in _dataContext.Specialities.Where(x => x.Id == dto.SpecialityId).DefaultIfEmpty()
          from c in _dataContext.Clinics.Where(x => x.Id == dto.ClinicId).DefaultIfEmpty()
          select new
          {
            Speciality = s,
            Clinic = c
          }
      ).AsNoTracking().FirstOrDefaultAsync();

      if (lookup?.Speciality is null)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "SpecialityId", Message = $"Speciality with id {dto.SpecialityId} was not found." });

      if (lookup.Clinic is null)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "ClinicId", Message = $"Clinic with id {dto.ClinicId} was not found." });

      var entity = new Doctor
      {
        Firstname = dto.Firstname.Trim(),
        Lastname = dto.Lastname.Trim(),
        ImageUrl = dto.ImageUrl,
        SpecialityId = dto.SpecialityId,
        ClinicId = dto.ClinicId
      };

      _dataContext.Doctors.Add(entity);
      try { await _dataContext.SaveChangesAsync(); }
      catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Create failed due to database constraint." }); }


      var response = new DoctorResponseDTO
      {
        Id = entity.Id,
        Firstname = entity.Firstname,
        Lastname = entity.Lastname,
        ImageUrl = entity.ImageUrl,
        SpecialityId = entity.SpecialityId,
        SpecialityName = lookup.Speciality.SpecialityName,
        ClinicId = entity.ClinicId,
        ClinicName = lookup.Clinic.ClinicName
      };

      return CreatedAtAction(nameof(GetDoctor), new { Id = response.Id }, response);

    }

    /// <summary>
    /// Updates a doctor by its ID.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "firstname": "Doc",
    ///   "lastname": "Tor",
    ///   "specialityId": 1,
    ///   "clinicId": 2
    /// }
    /// </remarks>
    /// <param name="Id">Doctor ID</param>
    /// <response code="204">Confirms update with status code.</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the doctor can't be found</response>
    /// <response code="409">Update failed due to database constraint.</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPut("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    [ProducesResponseType(typeof(ApiErrorDTO), 409)]
    public async Task<IActionResult> UpdateDoctor(Guid Id, UpdateDoctorDTO dto)
    {
      var entity = await _dataContext.Doctors.FindAsync(Id);
      if (entity == null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Doctor with id {Id} was not found."
        });
      }
      if (dto.SpecialityId is not null)
      {
        var speciality = await _dataContext.Specialities
      .AsNoTracking()
      .FirstOrDefaultAsync(s => s.Id == dto.SpecialityId);

        if (speciality is null) return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "SpecialityId", Message = $"Speciality with id {dto.SpecialityId} was not found." });

        entity.SpecialityId = dto.SpecialityId.Value;
      }
      if (dto.ClinicId is not null)
      {
        var clinic = await _dataContext.Clinics
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dto.ClinicId);

        if (clinic is null) return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "ClinicId", Message = $"Clinic with id {dto.ClinicId} was not found." });

        entity.ClinicId = dto.ClinicId.Value;
      }

      if (dto.Firstname is not null)
        entity.Firstname = dto.Firstname.Trim();

      if (dto.Lastname is not null)
        entity.Lastname = dto.Lastname.Trim();

      if (dto.ImageUrl is not null)
        entity.ImageUrl = dto.ImageUrl;

      try { await _dataContext.SaveChangesAsync(); }
      catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed due to database constraint." }); }

      return NoContent();
    }

    /// <summary>
    /// Deletes a doctor
    /// </summary>
    /// <param name="Id">Doctor ID</param>
    /// <response code="204">Confirms deletion with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the doctor can't be found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpDelete("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<IActionResult> DeleteDoctor(Guid Id)
    {
      var exists = await _dataContext.Doctors.AnyAsync(d => d.Id == Id);
      if (!exists)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Doctor with id {Id} was not found." });

      var hasAppointments = await _dataContext.Appointments.AnyAsync(a => a.DoctorId == Id);
      if (hasAppointments)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete doctor because they have appointments." });

      _dataContext.Doctors.Remove(new Doctor { Id = Id });

      try
      {
        await _dataContext.SaveChangesAsync();
        return NoContent();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete doctor because it is referenced by other records." });
      }
    }
  }
}