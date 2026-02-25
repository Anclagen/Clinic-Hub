using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using Backend.Extensions;

namespace Backend.Controllers
{
  [Route("appointments")]
  [Produces("application/json")]
  [Tags("Appointment")]
  [ApiController]
  public class AppointmentController : BaseApiController
  {
    private readonly DataContext _dataContext;

    public AppointmentController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    /// <summary>
    /// Retrieves all a patients appointments.
    /// </summary>
    /// <returns>A list of appointments</returns>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResponseDTO<AppointmentResponseDTO>), 200)]
    public async Task<IActionResult> GetAppointments([FromQuery] AppointmentQueryDTO q)
    {
      var page = Math.Max(q.Page, 1);
      var pageSize = Math.Clamp(q.PageSize, 1, 100);

      IQueryable<Appointment> query = _dataContext.Appointments.AsNoTracking();

      if (q.PatientId is Guid pid)
        query = query.Where(a => a.PatientId == pid);

      if (q.DoctorId is Guid did)
        query = query.Where(a => a.DoctorId == did);

      if (q.ClinicId is int cid)
        query = query.Where(a => a.ClinicId == cid);

      if (q.From.HasValue)
      {
        var from = q.From.Value;

        if (from.Kind != DateTimeKind.Utc)
          return BadRequest(new ApiBadRequestErrorDTO
          {
            StatusCode = 400,
            Field = "from",
            Message = "from must be UTC (Z)."
          });

        query = query.Where(a => a.StartAt >= from);
      }

      if (q.To.HasValue)
      {
        var to = q.To.Value;

        if (to.Kind != DateTimeKind.Utc)
          return BadRequest(new ApiBadRequestErrorDTO
          {
            StatusCode = 400,
            Field = "to",
            Message = "to must be UTC (Z)."
          });

        query = query.Where(a => a.StartAt < to);
      }


      if (q.From.HasValue && q.To.HasValue && q.To.Value <= q.From.Value)
        return BadRequest(new ApiBadRequestErrorDTO
        {
          StatusCode = 400,
          Field = "to",
          Message = "to must be greater than from."
        });

      var total = await query.CountAsync();

      var sortBy = (q.SortBy ?? "startAt").Trim().ToLowerInvariant();
      var sortDesc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

      query = sortBy switch
      {
        "startat" => sortDesc ? query.OrderByDescending(a => a.StartAt) : query.OrderBy(a => a.StartAt),
        _ => sortDesc ? query.OrderByDescending(a => a.StartAt) : query.OrderBy(a => a.StartAt),
      };

      query = query.Skip((page - 1) * pageSize).Take(pageSize);

      var data = await query
        .Select(a => new AppointmentResponseDTO
        {
          Id = a.Id,
          PatientId = a.PatientId,
          Firstname = a.Patient.Firstname,
          Lastname = a.Patient.Lastname,
          DateOfBirth = a.Patient.DateOfBirth,
          ClinicId = a.ClinicId,
          ClinicName = a.Clinic.ClinicName,
          DoctorId = a.DoctorId,
          DoctorName = a.Doctor.Firstname + " " + a.Doctor.Lastname,
          CategoryId = a.CategoryId,
          CategoryName = a.Category.CategoryName,
          Duration = a.DurationMinutes,
          StartAt = a.StartAt,
        })
        .ToListAsync();

      return Ok(new PagedResponseDTO<AppointmentResponseDTO>
      {
        Data = data,
        Pagination = new PaginationDTO
        {
          Page = page,
          PageSize = pageSize,
          Total = total,
          TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        }
      });
    }


    /// <summary>
    /// Retrieves all a patients appointments.
    /// </summary>
    /// <returns>A list of appointments</returns>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponseDTO<AppointmentResponseDTO>), 200)]
    public async Task<IActionResult> GetAppointments(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20
)
    {

      page = Math.Max(page, 1);
      pageSize = Math.Clamp(pageSize, 1, 100);

      var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
      if (!Guid.TryParse(sub, out var patientId))
        return Unauthorized();

      var query = _dataContext.Appointments
        .AsNoTracking()
        .Where(a => a.PatientId == patientId);
      var total = await query.CountAsync();
      var data = await query
          .OrderBy(d => d.StartAt)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .Select(d => new AppointmentResponseDTO
          {
            Id = d.Id,
            Firstname = d.Patient.Firstname,
            Lastname = d.Patient.Lastname,
            DateOfBirth = d.Patient.DateOfBirth,
            PatientId = d.PatientId,
            CategoryId = d.CategoryId,
            CategoryName = d.Category.CategoryName,
            Duration = d.DurationMinutes,
            StartAt = d.StartAt,
            ClinicId = d.ClinicId,
            ClinicName = d.Clinic.ClinicName,
            DoctorId = d.DoctorId,
            DoctorName = d.Doctor.Firstname + " " + d.Doctor.Lastname

          })
          .ToListAsync();

      return Ok(new PagedResponseDTO<AppointmentResponseDTO>
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
    /// Retrieves a appointment by ID.
    /// </summary>
    /// <param name="Id">The appointment ID.</param>
    /// <response code="200">Returns the appointment</response>
    /// <response code="404">If the appointment is not found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet("{Id}")]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentResponseDTO), 200)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<ActionResult<AppointmentResponseDTO>> GetAppointment(Guid Id)
    {
      var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
      if (!Guid.TryParse(sub, out var patientId)) return Unauthorized();

      var appointment = await _dataContext.Appointments.AsNoTracking()
          .Where(a => a.PatientId == patientId && a.Id == Id)
          .Select(a => new AppointmentResponseDTO
          {
            Id = a.Id,
            Firstname = a.Patient.Firstname,
            Lastname = a.Patient.Lastname,
            DateOfBirth = a.Patient.DateOfBirth,
            PatientId = a.PatientId,
            CategoryId = a.CategoryId,
            CategoryName = a.Category.CategoryName,
            Duration = a.DurationMinutes,
            ClinicId = a.ClinicId,
            ClinicName = a.Clinic.ClinicName,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor.Firstname + " " + a.Doctor.Lastname,
            StartAt = a.StartAt
          })
          .FirstOrDefaultAsync();
      if (appointment is null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Appointment with id {Id} was not found."
        });
      }
      return Ok(appointment);
    }

    /// <summary>
    /// Retrieves booked time slots for a doctor inside a date range.
    /// </summary>
    /// <param name="doctorId">Doctor identifier.</param>
    /// <param name="from">Inclusive range start (UTC).</param>
    /// <param name="to">Exclusive range end (UTC).</param>
    /// <response code="200">Returns booked slots for the given doctor and range.</response>
    /// <response code="400">If query parameters are invalid.</response>
    [HttpGet("booked-times")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<BookedTimeSlotDTO>), 200)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), 400)]
    public async Task<ActionResult<IEnumerable<BookedTimeSlotDTO>>> GetBookedTimes(
      [FromQuery] Guid doctorId,
      [FromQuery] DateTime from,
      [FromQuery] DateTime to
    )
    {
      if (doctorId == Guid.Empty)
      {
        return BadRequest(new ApiBadRequestErrorDTO
        {
          StatusCode = 400,
          Field = "doctorId",
          Message = "doctorId is required."
        });
      }

      if (from.Kind != DateTimeKind.Utc)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "from", Message = "from must be UTC (Z)." });

      if (to.Kind != DateTimeKind.Utc)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "to", Message = "to must be UTC (Z)." });

      if (to <= from)
      {
        return BadRequest(new ApiBadRequestErrorDTO
        {
          StatusCode = 400,
          Field = "to",
          Message = "to must be greater than from."
        });
      }

      if ((to - from).TotalDays > 31)
      {
        return BadRequest(new ApiBadRequestErrorDTO
        {
          StatusCode = 400,
          Field = "to",
          Message = "Date range cannot exceed 31 days."
        });
      }

      var doctorExists = await _dataContext.Doctors.AsNoTracking().AnyAsync(d => d.Id == doctorId);
      if (!doctorExists)
      {
        return BadRequest(new ApiBadRequestErrorDTO
        {
          StatusCode = 400,
          Field = "doctorId",
          Message = $"Doctor with id {doctorId} was not found."
        });
      }

      var slots = await _dataContext.Appointments
        .AsNoTracking()
        .Where(a =>
          a.DoctorId == doctorId &&
          a.StartAt < to &&
          a.StartAt.AddMinutes(a.DurationMinutes) > from
        )
        .OrderBy(a => a.StartAt)
        .Select(a => new BookedTimeSlotDTO
        {
          StartAt = a.StartAt,
          EndAt = a.StartAt.AddMinutes(a.DurationMinutes)
        })
        .ToListAsync();

      return Ok(slots);
    }

    /// <summary>
    /// Creates a appointment
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "firstname": "Doc",
    ///   "lastname": "Tor",
    ///   "specialityId": 1,
    ///   "clinicId": 2,
    ///   "startAt": "2026-02-25T21:08:30.000Z"
    /// }
    /// </remarks>
    /// <response code="201">Returns the newly created appointment</response>
    /// <response code="400">If the fullname is null</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="409">Create failed due to database constraint.</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AppointmentResponseDTO), 201)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppointmentResponseDTO>> AddAppointment([FromBody] CreateAppointmentDTO dto,
    [FromServices] IValidator<CreateAppointmentDTO> validator)
    {

      var result = await validator.ValidateAsync(dto);
      if (!result.IsValid)
      {
        return ValidationBadRequest(result);
      }

      var authHeader = Request.Headers.Authorization.ToString();
      var hasBearer = !string.IsNullOrWhiteSpace(authHeader) &&
                      authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

      var isAuthed = User?.Identity?.IsAuthenticated == true;

      if (hasBearer && !isAuthed)
      {
        return Unauthorized(new ApiErrorDTO
        {
          StatusCode = 401,
          Message = "Your session has expired. Please log in again."
        });
      }

      Guid? authenticatedPatientId = null;
      if (isAuthed)
      {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(sub, out var pid))
        {
          return Unauthorized(new ApiErrorDTO { StatusCode = 401, Message = "Invalid user identifier." });
        }
        authenticatedPatientId = pid;
      }

      var doctor = await _dataContext.Doctors.AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == dto.DoctorId);

      if (doctor is null)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "doctorId", Message = $"Doctor with id {dto.DoctorId} was not found." });

      var clinic = await _dataContext.Clinics.AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == dto.ClinicId);

      if (clinic is null)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "clinicId", Message = $"Clinic with id {dto.ClinicId} was not found." });

      var category = await _dataContext.Categories.AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == dto.CategoryId);

      if (category is null)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "categoryId", Message = $"Category with id {dto.CategoryId} was not found." });

      Patient? resolvedPatient = null;

      if (authenticatedPatientId is not null)
      {
        resolvedPatient = await _dataContext.Patients
          .SingleOrDefaultAsync(x => x.Id == authenticatedPatientId.Value);

        if (resolvedPatient is null)
          return Unauthorized(new ApiErrorDTO { StatusCode = 401, Message = "User account was not found." });

        if (resolvedPatient.IsDeleted)
          return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "", Message = "Your account is deleted. Please restore it or create a new account." });
      }
      else
      {
        resolvedPatient = await _dataContext.Patients.SingleOrDefaultAsync(p =>
          p.IsGuest &&
          !p.IsDeleted &&
          p.Firstname == dto.Firstname &&
          p.Lastname == dto.Lastname &&
          p.DateOfBirth == dto.DateOfBirth
        );

        if (resolvedPatient is null)
        {
          resolvedPatient = new Patient
          {
            Firstname = dto.Firstname.Trim(),
            Lastname = dto.Lastname.Trim(),
            IsGuest = true,
            IsDeleted = false,
            DateOfBirth = dto.DateOfBirth.Value
          };
        }
      }

      var newEnd = dto.StartAt.AddMinutes(dto.DurationMinutes);
      var overlaps = await _dataContext.Appointments.AnyAsync(a =>
        a.DoctorId == dto.DoctorId &&
        dto.StartAt < a.StartAt.AddMinutes(a.DurationMinutes) &&
        newEnd > a.StartAt
      );

      if (overlaps)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "This appointment coincides with another." });

      await using var tx = await _dataContext.Database.BeginTransactionAsync();

      try
      {
        if (resolvedPatient.Id == Guid.Empty)
        {
          _dataContext.Patients.Add(resolvedPatient);
          await _dataContext.SaveChangesAsync();
        }

        var entity = new Appointment
        {
          PatientId = resolvedPatient.Id,
          DoctorId = dto.DoctorId,
          ClinicId = dto.ClinicId,
          CategoryId = dto.CategoryId,
          DurationMinutes = dto.DurationMinutes,
          StartAt = dto.StartAt
        };

        _dataContext.Appointments.Add(entity);
        await _dataContext.SaveChangesAsync();

        await tx.CommitAsync();

        var response = new AppointmentResponseDTO
        {
          Id = entity.Id,
          PatientId = entity.PatientId,
          Firstname = resolvedPatient.Firstname,
          Lastname = resolvedPatient.Lastname,
          CategoryId = entity.CategoryId,
          CategoryName = category.CategoryName,
          DoctorId = entity.DoctorId,
          DoctorName = $"{doctor.Firstname} {doctor.Lastname}",
          ClinicId = entity.ClinicId,
          ClinicName = clinic.ClinicName,
          StartAt = entity.StartAt,
          Duration = entity.DurationMinutes,
        };

        return CreatedAtAction(nameof(GetAppointment), new { Id = response.Id }, response);
      }
      catch (DbUpdateException)
      {
        await tx.RollbackAsync();
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Create failed due to database constraint." });
      }
    }


    // /// <summary>
    // /// Updates a appointment by its ID.
    // /// </summary>
    // /// <remarks>
    // /// Sample request:
    // /// {
    // ///   "firstname": "Doc",
    // ///   "lastname": "Tor",
    // ///   "specialityId": 1,
    // ///   "clinicId": 2
    // /// }
    // /// </remarks>
    // /// <param name="Id">Appointment ID</param>
    // /// <response code="204">Confirms update with status code.</response>
    // /// <response code="400">Validation error</response>
    // /// <response code="401">If you lack an jwt token in your request headers</response>
    // /// <response code="404">If the appointment can't be found</response>
    // /// <response code="409">Update failed due to database constraint.</response>
    // /// <response code="500">Something went wrong server side.</response>
    // [HttpPut("{Id}")]
    // [Authorize]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    // [ProducesResponseType(typeof(ApiErrorDTO), 409)]
    // public async Task<IActionResult> UpdateAppointment(Guid Id, UpdateAppointmentDTO dto)
    // {
    //   var entity = await _dataContext.Appointments.FindAsync(Id);
    //   if (entity == null)
    //   {
    //     return NotFound(new ApiErrorDTO
    //     {
    //       StatusCode = 404,
    //       Message = $"Appointment with id {Id} was not found."
    //     });
    //   }
    //   if (dto.SpecialityId is not null)
    //   {
    //     var speciality = await _dataContext.Specialities
    //   .AsNoTracking()
    //   .FirstOrDefaultAsync(s => s.Id == dto.SpecialityId);

    //     if (speciality is null) return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "SpecialityId", Message = $"Speciality with id {dto.SpecialityId} was not found." });

    //     entity.SpecialityId = dto.SpecialityId.Value;
    //   }
    //   if (dto.ClinicId is not null)
    //   {
    //     var clinic = await _dataContext.Clinics
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(c => c.Id == dto.ClinicId);

    //     if (clinic is null) return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "ClinicId", Message = $"Clinic with id {dto.ClinicId} was not found." });

    //     entity.ClinicId = dto.ClinicId.Value;
    //   }

    //   if (dto.Firstname is not null)
    //     entity.Firstname = dto.Firstname.Trim();

    //   if (dto.Lastname is not null)
    //     entity.Lastname = dto.Lastname.Trim();

    //   if (dto.StartAt.Kind != DateTimeKind.Utc)
    // return BadRequest(new ApiBadRequestErrorDTO
    //   {
    //     StatusCode = 400,
    //     Field = "startAt",
    //     Message = "startAt must be UTC (Z)."
    //   });

    //   try { await _dataContext.SaveChangesAsync(); }
    //   catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed due to database constraint." }); }

    //   return NoContent();
    // }

    /// <summary>
    /// Deletes a appointment
    /// </summary>
    /// <param name="Id">Appointment ID</param>
    /// <response code="204">Confirms deletion with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the appointment can't be found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpDelete("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    [ProducesResponseType(typeof(ApiErrorDTO), 409)]
    public async Task<IActionResult> DeleteAppointment(Guid Id)
    {
      var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
      if (!Guid.TryParse(sub, out var patientId))
        return Unauthorized();

      var appointment = await _dataContext.Appointments
        .FirstOrDefaultAsync(a => a.Id == Id && a.PatientId == patientId);

      if (appointment is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Appointment with id {Id} was not found." });


      if (appointment.StartAt <= DateTime.UtcNow)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Past appointments cannot be deleted." });

      try
      {
        _dataContext.Appointments.Remove(appointment);
        await _dataContext.SaveChangesAsync();
        return NoContent();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete appointment because it is referenced by other records." });
      }
    }
  }
}
