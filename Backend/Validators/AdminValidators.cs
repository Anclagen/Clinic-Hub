using Backend.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public class CreateAdminValidator : AbstractValidator<CreateAdminDTO>
{
  public CreateAdminValidator(DataContext db)
  {
    RuleFor(x => x.Username)
      .Must(v => !string.IsNullOrWhiteSpace(v))
      .WithMessage("Username is required.")
      .Must(v => string.IsNullOrWhiteSpace(v) || v.Trim().Length <= 50)
      .WithMessage("Username must be 50 characters or fewer.")
      .MustAsync(async (username, cancellation) =>
      {
        if (string.IsNullOrWhiteSpace(username)) return true;
        var normalized = username.Trim().ToLower();
        return !await db.Admins.AnyAsync(a => a.Username.ToLower() == normalized, cancellation);
      })
      .WithMessage("Username already exists.");

    RuleFor(x => x.Email)
      .Must(v => !string.IsNullOrWhiteSpace(v))
      .WithMessage("Email is required.")
      .EmailAddress()
      .WithMessage("Email must be valid.")
      .Must(v => string.IsNullOrWhiteSpace(v) || v.Trim().Length <= 254)
      .WithMessage("Email must be 254 characters or fewer.")
      .MustAsync(async (email, cancellation) =>
      {
        if (string.IsNullOrWhiteSpace(email)) return true;
        var normalized = email.Trim().ToLower();
        return !await db.Admins.AnyAsync(a => a.Email.ToLower() == normalized, cancellation);
      })
      .WithMessage("Email already exists.");

    RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Password is required.")
      .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
  }
}

public class UpdateAdminValidator : AbstractValidator<UpdateAdminDTO>
{
  public UpdateAdminValidator()
  {
    RuleFor(x => x.Username)
      .Must(v => !string.IsNullOrWhiteSpace(v))
      .WithMessage("Username cannot be empty.")
      .MaximumLength(50)
      .WithMessage("Username must be 50 characters or fewer.")
      .When(x => x.Username != null);

    RuleFor(x => x.Email)
      .Must(v => !string.IsNullOrWhiteSpace(v))
      .WithMessage("Email cannot be empty.")
      .EmailAddress()
      .WithMessage("Email must be valid.")
      .MaximumLength(254)
      .WithMessage("Email must be 254 characters or fewer.")
      .When(x => x.Email != null);

    RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Password cannot be empty.")
      .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
      .When(x => x.Password != null);
  }
}
