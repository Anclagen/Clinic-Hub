using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;

public class AuthorizeOperationFilter : IOperationFilter
{
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    var hasAuthorize =
        context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
        context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;

    var hasAllowAnonymous =
        context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ||
        context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() == true;

    if (!hasAuthorize && !hasAllowAnonymous)
    {
      operation.Security = new List<OpenApiSecurityRequirement>();
    }
  }
}