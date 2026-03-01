using FluentValidation;
public class RegisterValidator : AbstractValidator<RegisterDTO>
{
  public RegisterValidator()
  {
    RuleFor(x => x.Email).NotEmpty().EmailAddress();
    RuleFor(x => x.Firstname).NotEmpty().MaximumLength(50);
    RuleFor(x => x.Lastname).NotEmpty().MaximumLength(50);

    // Enforce the 8-character minimum here
    RuleFor(x => x.Password)
        .NotEmpty().WithMessage("Password is required.")
        .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

    RuleFor(x => x.DateOfBirth)
        .NotEmpty()
        .Must(dob => dob <= DateOnly.FromDateTime(DateTime.Today))
        .WithMessage("Date of birth cannot be in the future.");
  }
}