using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.DTOs.Bookings;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Api.Controllers;

[Route("api/bookings")]
public class BookingsController : PublicControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>Creates a booking. A court is assigned randomly and only at this point (Rule #10).</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookingService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByReference), new { reference = result.Value!.BookingReference }, result.Value)
            : HandleFailure(result);
    }

    /// <summary>Public lookup — no account required, just the reference code from confirmation.</summary>
    [HttpGet("{reference}")]
    public async Task<IActionResult> GetByReference(string reference, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetByReferenceAsync(reference, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    /// <summary>Customer self-cancel, only before the booking's start time.</summary>
    [HttpPost("{reference}/cancel")]
    public async Task<IActionResult> Cancel(string reference, CancellationToken cancellationToken)
    {
        var result = await _bookingService.CancelAsync(reference, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
