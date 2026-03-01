using FluentValidation;
public class CreateClinicValidator : AbstractValidator<CreateClinicDTO>
{
  public CreateClinicValidator()
  {
    RuleFor(x => x.ClinicName)
        .NotEmpty().WithMessage("Clinic name is required.")
        .MaximumLength(100).WithMessage("Clinic name cannot exceed 100 characters.");

    RuleFor(x => x.Address)
        .MaximumLength(200)
        .When(x => x.Address != null);

    RuleFor(x => x.ImageUrl)
        .MaximumLength(500)
        .Must(LinkMustBeAtLeastHalfValid).WithMessage("ImageUrl must be a valid absolute URL.")
        .When(x => !string.IsNullOrEmpty(x.ImageUrl));

    RuleFor(x => x.ImageAlt)
        .MaximumLength(200)
        .When(x => x.ImageAlt != null);
  }

  private bool LinkMustBeAtLeastHalfValid(string? link)
  {
    if (string.IsNullOrWhiteSpace(link)) return true;
    return Uri.TryCreate(link, UriKind.Absolute, out var outUri)
           && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
  }
}

public class UpdateClinicValidator : AbstractValidator<UpdateClinicDTO>
{
  public UpdateClinicValidator()
  {
    RuleFor(x => x.ClinicName)
        .MaximumLength(100).WithMessage("Clinic name cannot exceed 100 characters.")
        .When(x => x.ClinicName != null);

    RuleFor(x => x.Address)
        .MaximumLength(200)
        .When(x => x.Address != null);

    RuleFor(x => x.ImageUrl)
        .MaximumLength(500)
        .Must(LinkMustBeAtLeastHalfValid).WithMessage("ImageUrl must be a valid absolute URL.")
        .When(x => !string.IsNullOrEmpty(x.ImageUrl));

    RuleFor(x => x.ImageAlt)
        .MaximumLength(200)
        .When(x => x.ImageAlt != null);
  }

  private bool LinkMustBeAtLeastHalfValid(string? link)
  {
    if (string.IsNullOrWhiteSpace(link)) return true;
    return Uri.TryCreate(link, UriKind.Absolute, out var outUri)
           && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
  }
}