using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Api.Controllers;

[Route("api/availability")]
public class AvailabilityController : PublicControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    /// <summary>
    /// Returns the bookable hourly slots for a date. Never includes court identity
    /// or count — only whether each hour can still be booked, and its price.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAvailability([FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var result = await _availabilityService.GetAvailabilityAsync(date, cancellationToken);
        return Ok(result);
    }
}
