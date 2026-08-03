using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.DTOs.Admin;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Api.Controllers.Admin;

[Route("api/admin/bookings")]
public class AdminBookingsController : AdminControllerBase
{
    private readonly IAdminBookingService _adminBookingService;

    public AdminBookingsController(IAdminBookingService adminBookingService)
    {
        _adminBookingService = adminBookingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? date,
        [FromQuery] BookingStatus? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var bookings = await _adminBookingService.GetAllAsync(date, status, search, cancellationToken);
        return Ok(bookings);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _adminBookingService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBookingStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _adminBookingService.UpdateStatusAsync(id, request.Status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
