using FluentValidation;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDTO>
{
    public CreateAppointmentValidator(DataContext db)
    {
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(5, 120)
            .WithMessage("Duration must be between 5 and 120 minutes.");

        RuleFor(x => x.StartAt)
            .Must(date => date.Kind == DateTimeKind.Utc).WithMessage("Start time must be in UTC.")
            .GreaterThan(DateTime.UtcNow).WithMessage("You cannot book appointments in the past.")
            .Must(d => d.Minute % 5 == 0 && d.Second == 0 && d.Millisecond == 0).WithMessage("StartAt must be a 5-minute slot.");

        RuleFor(x => x.DoctorId)
            .NotEmpty()
            .MustAsync(async (id, ct) => await db.Doctors.AnyAsync(d => d.Id == id, ct))
            .WithMessage(x => $"Doctor with id {x.DoctorId} was not found.");

        RuleFor(x => x.ClinicId)
            .GreaterThan(0)
            .MustAsync(async (id, ct) => await db.Clinics.AnyAsync(c => c.Id == id, ct))
            .WithMessage(x => $"Clinic with id {x.ClinicId} was not found.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .MustAsync(async (id, ct) => await db.Categories.AnyAsync(c => c.Id == id, ct))
            .WithMessage(x => $"Category with id {x.CategoryId} was not found.");

        When(x => x.PatientId == null, () =>
        {
            RuleFor(x => x.Firstname).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Lastname).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DateOfBirth).NotNull();
        });
    }
}

public class GetBookedTimesValidator : AbstractValidator<GetBookedTimesQuery>
{
    public GetBookedTimesValidator(DataContext db)
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty().WithMessage("doctorId is required.")
            .MustAsync(async (id, ct) => await db.Doctors.AnyAsync(d => d.Id == id, ct))
            .WithMessage(x => $"Doctor with id {x.DoctorId} was not found.");

        RuleFor(x => x.From)
            .Must(d => d.Kind == DateTimeKind.Utc).WithMessage("from must be UTC (Z).");

        RuleFor(x => x.To)
            .Must(d => d.Kind == DateTimeKind.Utc).WithMessage("to must be UTC (Z).")
            .GreaterThan(x => x.From).WithMessage("to must be greater than from.")
            .Must((query, to) => (to - query.From).TotalDays <= 31)
            .WithMessage("Date range cannot exceed 31 days.");
    }
}

public class AppointmentQueryValidator : AbstractValidator<AppointmentQueryDTO>
{
    public AppointmentQueryValidator()
    {
        RuleFor(x => x.Page).IsValidPageNumber();
        RuleFor(x => x.PageSize).IsValidPageSize();

        RuleFor(x => x.From)
            .Must(d => !d.HasValue || d.Value.Kind == DateTimeKind.Utc)
            .WithMessage("From date must be UTC.");

        RuleFor(x => x.To)
            .Must(d => !d.HasValue || d.Value.Kind == DateTimeKind.Utc)
            .WithMessage("To date must be UTC.")
            .GreaterThan(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("The 'To' date must be greater than the 'From' date.");
    }
}

public class UpdateAppointmentValidator : AbstractValidator<UpdateAppointmentDTO>
{
    public UpdateAppointmentValidator(DataContext db)
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty()
            .When(x => x.DoctorId != null);

        RuleFor(x => x.CategoryId)
            .MustAsync(async (id, ct) => await db.Categories.AnyAsync(c => c.Id == id, ct))
            .WithMessage("The selected category does not exist.")
            .When(x => x.CategoryId != null);

        RuleFor(x => x.StartAt)
            .Must(date => !date.HasValue || date.Value.Kind == DateTimeKind.Utc).WithMessage("Start time must be UTC.")
            .GreaterThan(DateTime.UtcNow.AddHours(24))
            .WithMessage("Appointments must be scheduled at least 24 hours in advance.")
            .When(x => x.StartAt.HasValue);

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(5, 120)
            .When(x => x.DurationMinutes != null);
    }
}