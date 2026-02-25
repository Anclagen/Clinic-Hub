using FluentValidation.Results;
using System.Text.Json;

namespace Backend.Extensions;

public static class ValidationExtensions
{
  public static Dictionary<string, string[]> ToCamelCaseDictionary(this FluentValidation.Results.ValidationResult result)
  {
    return result.Errors
        .GroupBy(e => e.PropertyName)
        .ToDictionary(
            g => JsonNamingPolicy.CamelCase.ConvertName(g.Key),
            g => g.Select(e => e.ErrorMessage).ToArray()
        );
  }
}