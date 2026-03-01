using FluentValidation;
public class CreateSpecialityValidator : AbstractValidator<CreateSpecialityDTO>
{
  public CreateSpecialityValidator()
  {
    RuleFor(x => x.SpecialityName)
        .NotEmpty().WithMessage("Speciality name is required.")
        .MaximumLength(100).WithMessage("Speciality name cannot exceed 100 characters.");

    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
        .When(x => x.Description != null);
  }
}

public class UpdateSpecialityValidator : AbstractValidator<UpdateSpecialityDTO>
{
  public UpdateSpecialityValidator()
  {
    RuleFor(x => x.SpecialityName)
        .NotEmpty().WithMessage("Speciality name is required.")
        .MaximumLength(100).WithMessage("Speciality name cannot exceed 100 characters.")
        .When(x => x.SpecialityName != null);

    RuleFor(x => x.Description)
        .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
        .When(x => x.Description != null);
  }
}