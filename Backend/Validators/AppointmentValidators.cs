using FluentValidation;
using Backend.Models;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDTO>
{
  public CreateAppointmentValidator()
  {
    RuleFor(x => x.DoctorId).NotEmpty();
    RuleFor(x => x.ClinicId).GreaterThan(0);
    RuleFor(x => x.CategoryId).GreaterThan(0);

    RuleFor(x => x.DurationMinutes)
        .InclusiveBetween(5, 120)
        .WithMessage("Duration must be between 5 and 120 minutes.");

    RuleFor(x => x.StartAt)
        .Must(date => date.Kind == DateTimeKind.Utc)
        .WithMessage("Start time must be in UTC.")
        .GreaterThan(DateTime.UtcNow)
        .WithMessage("You cannot book appointments in the past.")
        .Must(d => d.Minute % 5 == 0 && d.Second == 0 && d.Millisecond == 0)
        .WithMessage("StartAt must be a 5-minute slot.");

    When(x => x.PatientId == null, () =>
    {
      RuleFor(x => x.Firstname).NotEmpty().MaximumLength(100);
      RuleFor(x => x.Lastname).NotEmpty().MaximumLength(100);
      RuleFor(x => x.DateOfBirth).NotNull();
    });
  }
}