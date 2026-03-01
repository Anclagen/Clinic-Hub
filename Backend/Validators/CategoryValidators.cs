using FluentValidation;
public class CreateCategoryValidator : AbstractValidator<CreateCategoryDTO>
{
  public CreateCategoryValidator()
  {
    RuleFor(x => x.CategoryName)
        .NotEmpty().WithMessage("Category name is required.")
        .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");

    RuleFor(x => x.DefaultDuration)
        .InclusiveBetween(5, 120)
        .WithMessage("Duration must be between 5 and 120 minutes.")
        .Must(duration => duration % 5 == 0)
        .WithMessage("Duration must be in increments of 5 minutes (e.g., 5, 10, 15...).");

    RuleFor(x => x.Description)
        .MaximumLength(500)
        .When(x => !string.IsNullOrEmpty(x.Description));
  }

}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDTO>
{
  public UpdateCategoryValidator()
  {
    RuleFor(x => x.CategoryName)
        .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.")
        .When(x => x.CategoryName != null);

    RuleFor(x => x.DefaultDuration)
        .InclusiveBetween(5, 120)
        .WithMessage("Duration must be between 5 and 120 minutes.")
        .Must(duration => duration % 5 == 0)
        .WithMessage("Duration must be in increments of 5 minutes (e.g., 5, 10, 15...).")
        .When(x => x.DefaultDuration != null);

    RuleFor(x => x.Description)
        .MaximumLength(500)
        .When(x => !string.IsNullOrEmpty(x.Description));
  }
}