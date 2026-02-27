using FluentValidation;

public static class ValidationExtensions
{
  public static IRuleBuilderOptions<T, int> IsValidPageNumber<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder
        .GreaterThanOrEqualTo(1)
        .WithMessage("Page must be 1 or greater.");
  }

  public static IRuleBuilderOptions<T, int> IsValidPageSize<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder
        .InclusiveBetween(1, 100)
        .WithMessage("Page size must be between 1 and 100.");
  }

  public static IRuleBuilderOptions<T, DateTime?> IsUtcDate<T>(this IRuleBuilder<T, DateTime?> ruleBuilder)
  {
    return ruleBuilder
        .Must(d => !d.HasValue || d.Value.Kind == DateTimeKind.Utc)
        .WithMessage("Date must be UTC.");
  }
}