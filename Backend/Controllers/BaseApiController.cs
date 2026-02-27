using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Extensions;
namespace Backend.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
  protected ActionResult ValidationBadRequest(FluentValidation.Results.ValidationResult result)
  {
    var payload = new ApiErrorDTO
    {
      StatusCode = StatusCodes.Status400BadRequest,
      Message = "Validation failed.",
      Errors = result.ToCamelCaseDictionary()
    };

    return BadRequest(payload);
  }
}