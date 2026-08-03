using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.Common;

namespace PadelBooking.Api.Controllers;

[ApiController]
public abstract class PublicControllerBase : ControllerBase
{
    /// <summary>Maps a failed Result to the appropriate HTTP status code + ProblemDetails body.</summary>
    protected IActionResult HandleFailure<T>(Result<T> result)
    {
        var statusCode = result.ErrorType switch
        {
            ResultErrorType.NotFound => StatusCodes.Status404NotFound,
            ResultErrorType.Conflict => StatusCodes.Status409Conflict,
            ResultErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        var problem = new ProblemDetails
        {
            Title = result.ErrorType.ToString(),
            Detail = result.Error,
            Status = statusCode
        };

        return StatusCode(statusCode, problem);
    }
}
