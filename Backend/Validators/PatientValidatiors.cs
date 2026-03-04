using FluentValidation;
public class CreatePatientAdminValidator : AbstractValidator<CreatePatientAdminDto>
{
    public CreatePatientAdminValidator()
    {
        RuleFor(x => x.Firstname)
          .NotEmpty()
          .MaximumLength(100);

        RuleFor(x => x.Lastname)
          .NotEmpty()
          .MaximumLength(100);

        When(x => x.Email != null, () =>
        {
            RuleFor(x => x.Email!)
          .EmailAddress()
          .MaximumLength(255);
        });

        RuleFor(x => x.DateOfBirth)
          .NotNull()
          .WithMessage("Date of birth is required for registered patients.");
    }
}
public class UpdatePatientValidator : AbstractValidator<UpdatePatientDto>
{
    public UpdatePatientValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email is required.")
            .When(x => x.Email != null);

        RuleFor(x => x.Firstname)
            .MaximumLength(50)
            .When(x => x.Firstname != null);

        RuleFor(x => x.Lastname)
            .MaximumLength(50)
            .When(x => x.Lastname != null);

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Date of Birth cannot be in the future.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.SocialSecurityNumber)
                    .Matches(@"^\d{11}$").WithMessage("Birth number (Fødselsnummer) must be 11 digits.")
                    .When(x => !string.IsNullOrEmpty(x.SocialSecurityNumber));

        RuleFor(x => x.Gender)
            .Must(g => new[] { "Male", "Female", "Other" }.Contains(g))
            .WithMessage("Please select a valid gender.")
            .When(x => !string.IsNullOrEmpty(x.Gender));

        RuleFor(x => x.MedicalInsuranceMemberNumber)
                    .Matches(@"^\d+$").WithMessage("Insurance number must be numeric.")
                    .MaximumLength(20)
                    .When(x => !string.IsNullOrEmpty(x.MedicalInsuranceMemberNumber));

        RuleFor(x => x.Address)
            .MaximumLength(200)
            .When(x => x.Address != null);
    }
}

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDTO>
{
    public ChangePasswordValidator()
    {
        // We only care that they provided the old one
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Current password is required.");

        // We enforce the 8-character rule ONLY on the new one
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long.")
            .NotEqual(x => x.OldPassword).WithMessage("New password cannot be the same as the old one.");
    }
}