using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.DTOs.Auth;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Authenticates an admin user and returns a JWT bearer token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Login failed",
                Detail = result.Error,
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Ok(result.Value);
    }
}
