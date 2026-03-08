using FluentValidation;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

public class CreateDoctorValidator : AbstractValidator<CreateDoctorDTO>
{
    private readonly DataContext _db;

    public CreateDoctorValidator(DataContext db)
    {
        _db = db;

        RuleFor(x => x.Firstname)
            .Must(v => !string.IsNullOrWhiteSpace(v))
            .WithMessage("Firstname is required.")
            .Must(v => string.IsNullOrWhiteSpace(v) || v.Trim().Length <= 50)
            .WithMessage("Firstname must be 50 characters or fewer.");

        RuleFor(x => x.Lastname)
            .Must(v => !string.IsNullOrWhiteSpace(v))
            .WithMessage("Lastname is required.")
            .Must(v => string.IsNullOrWhiteSpace(v) || v.Trim().Length <= 50)
            .WithMessage("Lastname must be 50 characters or fewer.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .Must(LinkMustBeAtLeastHalfValid).WithMessage("ImageUrl must be a valid absolute URL.")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.SpecialityId)
            .MustAsync(async (id, cancellation) => await _db.Specialities.AnyAsync(s => s.Id == id))
            .WithMessage("Speciality does not exist.");

        RuleFor(x => x.ClinicId)
            .MustAsync(async (id, cancellation) => await _db.Clinics.AnyAsync(c => c.Id == id))
            .WithMessage("Clinic does not exist.");
    }

    private bool LinkMustBeAtLeastHalfValid(string? link)
    {
        if (string.IsNullOrWhiteSpace(link)) return true;
        return Uri.TryCreate(link, UriKind.Absolute, out var outUri)
               && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }
}

public class UpdateDoctorValidator : AbstractValidator<UpdateDoctorDTO>
{
    private readonly DataContext _db;

    public UpdateDoctorValidator(DataContext db)
    {
        _db = db;

        RuleFor(x => x.Firstname)
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
            .When(x => x.Firstname != null);

        RuleFor(x => x.Lastname)
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
            .When(x => x.Lastname != null);

        RuleFor(x => x.SpecialityId)
            .MustAsync(async (id, cancellation) => await _db.Specialities.AnyAsync(s => s.Id == id))
            .When(x => x.SpecialityId.HasValue)
            .WithMessage("The specified Speciality does not exist.");

        RuleFor(x => x.ClinicId)
            .MustAsync(async (id, cancellation) => await _db.Clinics.AnyAsync(c => c.Id == id))
            .When(x => x.ClinicId.HasValue)
            .WithMessage("The specified Clinic does not exist.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .Must(LinkMustBeAtLeastHalfValid).WithMessage("ImageUrl must be a valid absolute URL.")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }

    private bool LinkMustBeAtLeastHalfValid(string? link)
    {
        if (string.IsNullOrWhiteSpace(link)) return true;
        return Uri.TryCreate(link, UriKind.Absolute, out var outUri)
               && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps);
    }
}