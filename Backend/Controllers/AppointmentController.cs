using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Backend.Controllers
{
  [Route("appointments")]
  [Produces("application/json")]
  [Tags("Appointment")]
  [ApiController]
  public class AppointmentController : ControllerBase
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
    [Authorize]
    [ProducesResponseType(typeof(PagedResponseDTO<AppointmentResponseDTO>), 200)]
    public async Task<IActionResult> GetAppointments(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20
)
    {
      var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
      page = Math.Max(page, 1);
      pageSize = Math.Clamp(pageSize, 1, 100);

      var query = _dataContext.Appointments.AsNoTracking().AsQueryable();
      query = query.Where(a => a.PatientId.ToString() == sub);
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
      var query = _dataContext.Appointments.AsNoTracking().AsQueryable();
      query = query.Where(a => a.PatientId.ToString() == sub);
      var appointment = await query
          .Where(a => a.Id == Id)
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
            DoctorName = a.Doctor.Firstname + " " + a.Doctor.Lastname
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
    /// <param name="from">Inclusive range start (UTC/local DateTime).</param>
    /// <param name="to">Exclusive range end (UTC/local DateTime).</param>
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
    ///   "clinicId": 2
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
    public async Task<ActionResult<AppointmentResponseDTO>> AddAppointment([FromBody] CreateAppointmentDTO dto)
    {
      // 1) If they send a Bearer token, it must be valid. No silent downgrade to guest.
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

      // 2) Validate referenced entities (separately, predictably)
      var doctor = await _dataContext.Doctors.AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == dto.DoctorId);

      if (doctor is null)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "DoctorId", Message = $"Doctor with id {dto.DoctorId} was not found." });

      var clinic = await _dataContext.Clinics.AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == dto.ClinicId);

      if (clinic is null)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "ClinicId", Message = $"Clinic with id {dto.ClinicId} was not found." });

      var category = await _dataContext.Categories.AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == dto.CategoryId);

      if (category is null)
        return BadRequest(new ApiBadRequestErrorDTO { StatusCode = 400, Field = "CategoryId", Message = $"Category with id {dto.CategoryId} was not found." });

      // 3) Resolve patient (JWT user OR guest)
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
        if (string.IsNullOrWhiteSpace(dto.Firstname) ||
            string.IsNullOrWhiteSpace(dto.Lastname) ||
            dto.DateOfBirth is null)
        {
          return BadRequest(new ApiBadRequestErrorDTO
          {
            StatusCode = 400,
            Field = "Firstname, Lastname, DateOfBirth",
            Message = "A firstname, lastname, and date of birth is required for unregistered patients."
          });
        }

        // Optional but sane: reuse an existing guest instead of creating duplicates.
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

      // 4) Overlap check (still not perfectly race-safe, but fine for exam-level logic)
      var newEnd = dto.StartAt.AddMinutes(dto.DurationMinutes);
      var overlaps = await _dataContext.Appointments.AnyAsync(a =>
        a.DoctorId == dto.DoctorId &&
        dto.StartAt < a.StartAt.AddMinutes(a.DurationMinutes) &&
        newEnd > a.StartAt
      );

      if (overlaps)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "This appointment coincides with another." });

      // 5) Transaction: avoid creating a guest without an appointment if insert fails
      await using var tx = await _dataContext.Database.BeginTransactionAsync();

      try
      {
        // If guest was new (not found), it isn't tracked yet. Add it now.
        if (resolvedPatient.Id == Guid.Empty) // assuming Guid PK generated by EF on add
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

    //   try { await _dataContext.SaveChangesAsync(); }
    //   catch (DbUpdateException) { return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Update failed due to database constraint." }); }

    //   return NoContent();
    // }

    // /// <summary>
    // /// Deletes a appointment
    // /// </summary>
    // /// <param name="Id">Appointment ID</param>
    // /// <response code="204">Confirms deletion with status code.</response>
    // /// <response code="401">If you lack an jwt token in your request headers</response>
    // /// <response code="404">If the appointment can't be found</response>
    // /// <response code="500">Something went wrong server side.</response>
    // [HttpDelete("{Id}")]
    // [Authorize]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    // public async Task<IActionResult> DeleteAppointment(Guid Id)
    // {
    //   var exists = await _dataContext.Appointments.AnyAsync(d => d.Id == Id);
    //   if (!exists)
    //     return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Appointment with id {Id} was not found." });

    //   var hasAppointments = await _dataContext.Appointments.AnyAsync(a => a.AppointmentId == Id);
    //   if (hasAppointments)
    //     return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete appointment because they have appointments." });

    //   _dataContext.Appointments.Remove(new Appointment { Id = Id });

    //   try
    //   {
    //     await _dataContext.SaveChangesAsync();
    //     return NoContent();
    //   }
    //   catch (DbUpdateException)
    //   {
    //     return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete appointment because it is referenced by other records." });
    //   }
    // }
  }
}
