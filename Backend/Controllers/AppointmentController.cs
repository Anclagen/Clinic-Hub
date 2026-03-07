using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using FluentValidation;

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
    /// Retrieves a paged and filtered list of all appointments (Admin only).
    /// </summary>
    /// <remarks>
    /// **Administrative Controls:**
    /// - **Filtering:** Filter by Patient, Doctor, Clinic, or Date Range.
    /// - **Sorting:** Supports sorting by `patientname`, `doctorname`, `clinicname`, or `startAt` (default).
    /// - **Performance:** Uses keyset-adjacent offset paging. Max page size is 100.
    /// </remarks>
    /// <param name="q">Query parameters including filters, pagination, and sorting.</param>
    /// <param name="validator">Injected validator for date ranges and GUID formats.</param>
    /// <response code="200">Returns a paged wrapper of all appointments matching the criteria.</response>
    /// <response code="400">Bad Request: Validation failed (e.g., invalid GUID or 'From' date after 'To' date).</response>
    /// <response code="401">Unauthorized: Token is missing or invalid.</response>
    /// <response code="403">Forbidden: User does not have the 'Admin' role.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResponseDTO<AppointmentResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] AppointmentQueryDTO q,
        [FromServices] IValidator<AppointmentQueryDTO> validator)
    {
      // 1. Validate Query Logic
      var result = await validator.ValidateAsync(q);
      if (!result.IsValid) return ValidationBadRequest(result);

      IQueryable<Appointment> query = _dataContext.Appointments.AsNoTracking();
      if (q.PatientId.HasValue) query = query.Where(a => a.PatientId == q.PatientId);
      if (q.DoctorId.HasValue) query = query.Where(a => a.DoctorId == q.DoctorId);
      if (q.ClinicId.HasValue) query = query.Where(a => a.ClinicId == q.ClinicId);
      if (q.From.HasValue) query = query.Where(a => a.StartAt >= q.From);
      if (q.To.HasValue) query = query.Where(a => a.StartAt < q.To);

      var total = await query.CountAsync();

      var isDesc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
      query = (q.SortBy?.ToLower()) switch
      {
        "patientname" => isDesc ? query.OrderByDescending(a => a.Patient.Lastname) : query.OrderBy(a => a.Patient.Lastname),
        "doctorname" => isDesc ? query.OrderByDescending(a => a.Doctor.Lastname) : query.OrderBy(a => a.Doctor.Lastname),
        "clinicname" => isDesc ? query.OrderByDescending(a => a.Clinic.ClinicName) : query.OrderBy(a => a.Clinic.ClinicName),
        _ => isDesc ? query.OrderByDescending(a => a.StartAt) : query.OrderBy(a => a.StartAt),
      };

      var data = await query
          .Skip((q.Page - 1) * q.PageSize)
          .Take(q.PageSize)
          .Select(a => new AppointmentResponseDTO
          {
            Id = a.Id,
            PatientId = a.PatientId,
            Firstname = a.Patient.Firstname,
            Lastname = a.Patient.Lastname,
            DateOfBirth = a.Patient.DateOfBirth,
            Email = a.Patient.Email,
            ClinicId = a.ClinicId,
            ClinicName = a.Clinic.ClinicName,
            DoctorId = a.DoctorId,
            DoctorName = $"{a.Doctor.Firstname} {a.Doctor.Lastname}",
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
          Page = q.Page,
          PageSize = q.PageSize,
          Total = total,
          TotalPages = (int)Math.Ceiling(total / (double)q.PageSize)
        }
      });
    }


    /// <summary>
    /// Retrieves a paged list of the authenticated patient's appointments.
    /// </summary>
    /// <remarks>
    /// **Privacy:** Automatically filters by the 'sub' claim in the JWT to ensure patients only see their own data.
    /// **Default Sort:** Ordered by StartAt (Ascending).
    /// </remarks>
    /// <param name="page">The page number (minimum 1).</param>
    /// <param name="pageSize">Number of items per page (maximum 100).</param>
    /// <response code="200">Returns a paged wrapper containing the patient's appointments.</response>
    /// <response code="401">Unauthorized: Valid JWT is required.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponseDTO<AppointmentResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAppointments(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 100
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
          .OrderByDescending(d => d.StartAt)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .Select(d => new AppointmentResponseDTO
          {
            Id = d.Id,
            Firstname = d.Patient.Firstname,
            Lastname = d.Patient.Lastname,
            DateOfBirth = d.Patient.DateOfBirth,
            Email = d.Patient.Email,
            PatientId = d.PatientId,
            CategoryId = d.CategoryId,
            CategoryName = d.Category.CategoryName,
            Duration = d.DurationMinutes,
            StartAt = DateTime.SpecifyKind(d.StartAt, DateTimeKind.Utc),
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
    /// Retrieves details for a specific appointment.
    /// </summary>
    /// <remarks>
    /// **Security:** /// Only the patient who owns the appointment can access this data. 
    /// If the appointment belongs to a different user, a 404 is returned to prevent resource enumeration.
    /// </remarks>
    /// <param name="id">The unique GUID of the appointment.</param>
    /// <response code="200">Returns the full appointment details.</response>
    /// <response code="401">Unauthorized: Valid JWT token is required.</response>
    /// <response code="404">Not Found: Appointment doesn't exist or doesn't belong to the authenticated user.</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentResponseDTO>> GetAppointment(Guid id)
    {
      var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
      if (!Guid.TryParse(sub, out var patientId)) return Unauthorized();

      var appointment = await _dataContext.Appointments.AsNoTracking()
          .Where(a => a.PatientId == patientId && a.Id == id)
          .Select(a => new AppointmentResponseDTO
          {
            Id = a.Id,
            Firstname = a.Patient.Firstname,
            Lastname = a.Patient.Lastname,
            DateOfBirth = a.Patient.DateOfBirth,
            Email = a.Patient.Email,
            PatientId = a.PatientId,
            CategoryId = a.CategoryId,
            CategoryName = a.Category.CategoryName,
            Duration = a.DurationMinutes,
            ClinicId = a.ClinicId,
            ClinicName = a.Clinic.ClinicName,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor.Firstname + " " + a.Doctor.Lastname,
            StartAt = DateTime.SpecifyKind(a.StartAt, DateTimeKind.Utc)
          })
          .FirstOrDefaultAsync();
      if (appointment is null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Appointment with id {id} was not found."
        });
      }
      return Ok(appointment);
    }

    /// <summary>
    /// Retrieves a doctor's booked time slots within a specific date range.
    /// </summary>
    /// <remarks>
    /// **Purpose:** Used by the frontend calendar to "gray out" unavailable slots.
    /// 
    /// **Rules:**
    /// - **Timezone:** All dates MUST be in UTC (ending with 'Z').
    /// - **Limit:** The range between 'From' and 'To' cannot exceed 31 days.
    /// - **Privacy:** Only returns time intervals; no patient data is exposed.
    /// </remarks>
    /// <param name="query">The criteria including DoctorId and the UTC date range.</param>
    /// <param name="validator">Injected validator for range and UTC format checks.</param>
    /// <response code="200">Returns a list of Start and End times for existing bookings.</response>
    /// <response code="400">Bad Request: Invalid GUID, non-UTC dates, or range exceeds 31 days.</response>
    [HttpGet("booked-times")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<BookedTimeSlotDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<BookedTimeSlotDTO>>> GetBookedTimes(
        [FromQuery] GetBookedTimesQuery query,
        [FromServices] IValidator<GetBookedTimesQuery> validator)
    {
      var result = await validator.ValidateAsync(query);
      if (!result.IsValid)
      {
        return ValidationBadRequest(result);
      }

      var slots = await _dataContext.Appointments
              .AsNoTracking()
              .Where(a =>
                  a.DoctorId == query.DoctorId &&
                  a.StartAt < query.To &&
                  a.StartAt.AddMinutes(a.DurationMinutes) > query.From
              )
              .OrderBy(a => a.StartAt)
              .Select(a => new BookedTimeSlotDTO
              {
                StartAt = DateTime.SpecifyKind(a.StartAt, DateTimeKind.Utc),
                EndAt = DateTime.SpecifyKind(a.StartAt.AddMinutes(a.DurationMinutes), DateTimeKind.Utc)
              })
              .ToListAsync();

      return Ok(slots);
    }

    /// <summary>
    /// Books a new appointment.
    /// </summary>
    /// <remarks>
    /// **Behavior:**
    /// - **Authenticated Users:** If a valid JWT is provided, the appointment is linked to the logged-in user.
    /// - **Guest Users:** If no token is provided, the system searches for an existing guest profile matching the name and DOB. If none is found, a new guest profile is created.
    /// - **Validation:** Ensures the doctor belongs to the clinic and that the time slot is currently available.
    /// </remarks>
    /// <param name="dto">The appointment details including Patient info (for guests), Doctor, and Schedule.</param>
    /// <param name="coreValidator">Injected FluentValidator for CreateAppointmentDTO.</param>
    /// <param name="guestValidator">Injected FluentValidator for anonymous Creation.</param>
    /// <response code="201">Success: Returns the newly created appointment with full details.</response>
    /// <response code="400">Bad Request: Validation failed or account is deleted.</response>
    /// <response code="401">Unauthorized: Session expired or invalid token detected.</response>
    /// <response code="409">Conflict: The selected time slot is already booked for this doctor.</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AppointmentResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentResponseDTO>> AddAppointment(
    [FromBody] CreateAppointmentDTO dto,
    [FromServices] IValidator<CreateAppointmentDTO> coreValidator,
    [FromServices] IValidator<CreateAnonymousAppointmentDTO> guestValidator)
    {
      // Throw out expired/invalid Bearer tokens immediately
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

      // Who is booking this? if logged in get the id
      Guid? authenticatedPatientId = null;
      if (isAuthed)
      {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (Guid.TryParse(sub, out var pid))
          authenticatedPatientId = pid;
        else
          return Unauthorized(new ApiErrorDTO { Message = "Invalid user identifier." });
      }

      // If not logged in, check first, last name, dob, and email fields before hitting the DB
      if (authenticatedPatientId == null)
      {
        var guestResult = await guestValidator.ValidateAsync(dto);
        if (!guestResult.IsValid) return ValidationBadRequest(guestResult);
      }

      // Now hit the DB to check Doctor/Clinic existence
      var coreResult = await coreValidator.ValidateAsync(dto);
      if (!coreResult.IsValid) return ValidationBadRequest(coreResult);

      // get or setup the patient creation
      Patient? resolvedPatient = null;
      if (authenticatedPatientId.HasValue)
      {
        resolvedPatient = await _dataContext.Patients.FindAsync(authenticatedPatientId.Value);
        // chuck out deleted or anonymised accounts that might have a token still floating around, frontend should logout on 401.
        if (resolvedPatient == null || resolvedPatient.IsDeleted)
          return Unauthorized(new ApiErrorDTO { StatusCode = 401, Message = "Account is invalid or deactivated." });
      }
      else
      {
        // Search for existing Guest record to avoid duplicates, and push matches to current guest user in the system
        var emailLower = dto.Email!.Trim().ToLower();
        var existing = await _dataContext.Patients
            .FirstOrDefaultAsync(p => p.Email == emailLower && !p.IsDeleted);

        if (existing != null)
        {
          // If they have a real account, they shouldn't book as a guest
          if (!existing.IsGuest)
          {
            return Conflict(new ApiErrorDTO
            {
              Message = "This email is registered. Please log in to book your appointment."
            });
          }

          // If guest, verify matching details as well as email.
          bool matches = existing.Firstname.Equals(dto.Firstname?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                             existing.Lastname.Equals(dto.Lastname?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                             existing.DateOfBirth == dto.DateOfBirth;

          if (!matches)
          {
            return Conflict(new ApiErrorDTO
            {
              Message = "This email is associated with a different guest profile (Name or DOB mismatch)."
            });
          }

          resolvedPatient = existing;
        }
        else
        {
          resolvedPatient = new Patient
          {
            Firstname = dto.Firstname!.Trim(),
            Lastname = dto.Lastname!.Trim(),
            Email = emailLower,
            DateOfBirth = dto.DateOfBirth,
            IsGuest = true,
            IsDeleted = false
          };
        }
      }

      // booking conflicts
      var newEnd = dto.StartAt.AddMinutes(dto.DurationMinutes);
      var overlaps = await _dataContext.Appointments.AnyAsync(a =>
        a.DoctorId == dto.DoctorId &&
        dto.StartAt < a.StartAt.AddMinutes(a.DurationMinutes) &&
        newEnd > a.StartAt
      );

      if (overlaps)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "This appointment coincides with another." });

      if (resolvedPatient.Id != Guid.Empty)
      {
        var overlapsPatient = await _dataContext.Appointments.AnyAsync(a =>
            a.PatientId == resolvedPatient.Id &&
            dto.StartAt < a.StartAt.AddMinutes(a.DurationMinutes) &&
            newEnd > a.StartAt);

        if (overlapsPatient)
          return Conflict(new ApiErrorDTO
          {
            StatusCode = 409,
            Message = "You already have an appointment overlapping this time slot."
          });
      }

      // Transaction time!!!
      await using var tx = await _dataContext.Database.BeginTransactionAsync();
      try
      {
        // add patient if they have no id and need creating
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

        var finalAppointment = await _dataContext.Appointments
          .Include(a => a.Category)
          .Include(a => a.Doctor)
          .Include(a => a.Clinic)
          .Include(a => a.Patient)
          .FirstAsync(a => a.Id == entity.Id);

        var response = new AppointmentResponseDTO
        {
          Id = finalAppointment.Id,
          PatientId = finalAppointment.PatientId,
          Firstname = finalAppointment.Patient.Firstname,
          Lastname = finalAppointment.Patient.Lastname,
          Email = finalAppointment.Patient.Email,
          DateOfBirth = finalAppointment.Patient.DateOfBirth,
          CategoryId = finalAppointment.CategoryId,
          CategoryName = finalAppointment.Category.CategoryName,
          DoctorId = finalAppointment.DoctorId,
          DoctorName = $"{finalAppointment.Doctor.Firstname} {finalAppointment.Doctor.Lastname}",
          ClinicId = finalAppointment.ClinicId,
          ClinicName = finalAppointment.Clinic.ClinicName,
          StartAt = DateTime.SpecifyKind(finalAppointment.StartAt, DateTimeKind.Utc),
          Duration = finalAppointment.DurationMinutes,
        };

        return CreatedAtAction("GetAppointment", new { Id = entity.Id }, response);
      }
      catch (DbUpdateException)
      {
        await tx.RollbackAsync();
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Create failed due to database constraint." });
      }
    }


    /// <summary>
    /// Updates an existing appointment's schedule or doctor.
    /// </summary>
    /// <remarks>
    /// **Constraints:**
    /// - **Ownership:** Only the patient who booked the appointment can modify it.
    /// - **Time Lock:** Modifications are forbidden within 24 hours of the original start time.
    /// - **Clinic Lock:** The doctor must belong to the same clinic as the original appointment.
    /// - **Availability:** The new time slot must not overlap with the selected doctor's existing schedule.
    /// **Partial Updates:**
    /// Only include the fields you wish to change. Null fields will retain their current values.
    /// </remarks>
    /// <param name="id">The unique GUID of the appointment.</param>
    /// <param name="dto">The fields to update.</param>
    /// <param name="validator">Injected FluentValidator for UpdateAppointmentDTO.</param>
    /// <response code="200">Success: Returns the full updated appointment details.</response>
    /// <response code="400">Bad Request: Validation failed or modification attempted within the 24-hour lock period.</response>
    /// <response code="404">Not Found: Appointment ID is invalid or does not belong to the user.</response>
    /// <response code="409">Conflict: The selected doctor has a scheduling overlap.</response>
    [HttpPatch("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(AppointmentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentResponseDTO>> UpdateAppointment(
        Guid id,
        [FromBody] UpdateAppointmentDTO dto,
        [FromServices] IValidator<UpdateAppointmentDTO> validator)
    {
      var validationResult = await validator.ValidateAsync(dto);
      if (!validationResult.IsValid) return ValidationBadRequest(validationResult);

      var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
      if (!Guid.TryParse(sub, out var patientId)) return Unauthorized();

      var appointment = await _dataContext.Appointments
          .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patientId);

      if (appointment == null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = "Appointment not found." });

      if (appointment.StartAt <= DateTime.UtcNow.AddHours(24))
        return BadRequest(new ApiErrorDTO { StatusCode = 400, Message = "Appointments cannot be modified within 24 hours of their start time." });

      if (dto.CategoryId.HasValue) appointment.CategoryId = dto.CategoryId.Value;
      if (dto.DoctorId.HasValue)
      {
        var doctorExistsAtClinic = await _dataContext.Doctors
          .AnyAsync(d => d.Id == dto.DoctorId && d.ClinicId == appointment.ClinicId);

        if (!doctorExistsAtClinic)
          return BadRequest(new ApiBadRequestErrorDTO { Field = "DoctorId", Message = "Selected doctor does not practice at the clinic associated with this appointment." });

        appointment.DoctorId = dto.DoctorId.Value;
      }
      if (dto.StartAt.HasValue) appointment.StartAt = dto.StartAt.Value;
      if (dto.DurationMinutes.HasValue) appointment.DurationMinutes = dto.DurationMinutes.Value;

      var newEnd = appointment.StartAt.AddMinutes(appointment.DurationMinutes);
      var overlaps = await _dataContext.Appointments.AnyAsync(a =>
                  a.Id != id &&
                  a.DoctorId == appointment.DoctorId &&
                  appointment.StartAt < a.StartAt.AddMinutes(a.DurationMinutes) &&
                  newEnd > a.StartAt);
      if (overlaps)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "The selected doctor is already booked for this time slot." });

      var overlapsPatient = await _dataContext.Appointments.AnyAsync(a =>
          a.Id != id &&
          a.PatientId == appointment.PatientId &&
          appointment.StartAt < a.StartAt.AddMinutes(a.DurationMinutes) &&
          newEnd > a.StartAt);

      if (overlapsPatient)
        return Conflict(new ApiErrorDTO
        {
          StatusCode = 409,
          Message = "You already have an appointment overlapping this time slot."
        });

      await _dataContext.SaveChangesAsync();

      var updatedAppointment = await _dataContext.Appointments
          .Include(a => a.Category)
          .Include(a => a.Doctor)
          .Include(a => a.Clinic)
          .Include(a => a.Patient)
          .Select(a => new AppointmentResponseDTO
          {
            Id = a.Id,
            PatientId = a.PatientId,
            Firstname = a.Patient.Firstname,
            Lastname = a.Patient.Lastname,
            DateOfBirth = a.Patient.DateOfBirth,
            CategoryId = a.CategoryId,
            CategoryName = a.Category.CategoryName,
            DoctorId = a.DoctorId,
            DoctorName = $"{a.Doctor.Firstname} {a.Doctor.Lastname}",
            ClinicId = a.ClinicId,
            ClinicName = a.Clinic.ClinicName,
            StartAt = DateTime.SpecifyKind(a.StartAt, DateTimeKind.Utc),
            Duration = a.DurationMinutes
          })
          .FirstAsync(a => a.Id == id);

      return Ok(updatedAppointment);
    }

    /// <summary>
    /// Cancels and deletes a specific appointment.
    /// </summary>
    /// <remarks>
    /// **Rules:**
    /// 1. Only the patient who owns the appointment can delete it.
    /// 2. Appointments cannot be deleted if they are in the past.
    /// 3. Appointments cannot be deleted within 24 hours of the start time (Cancellation Lock).
    /// </remarks>
    /// <param name="id">The unique GUID of the appointment.</param>
    /// <response code="204">Appointment successfully deleted.</response>
    /// <response code="401">Unauthorized: Token is missing or invalid.</response>
    /// <response code="404">Not Found: Appointment doesn't exist or doesn't belong to the user.</response>
    /// <response code="409">Conflict: Too late to cancel or database constraint violation.</response>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 401)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    [ProducesResponseType(typeof(ApiErrorDTO), 409)]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
      var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
      if (!Guid.TryParse(sub, out var patientId))
        return Unauthorized();

      var appointment = await _dataContext.Appointments
        .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patientId);

      if (appointment is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = "Appointment was not found." });


      if (appointment.StartAt <= DateTime.UtcNow)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Past appointments cannot be deleted." });

      if (appointment.StartAt <= DateTime.UtcNow.AddHours(24))
      {
        return Conflict(new ApiErrorDTO
        {
          StatusCode = 409,
          Message = "Appointments cannot be cancelled within 24 hours of the start time."
        });
      }

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
