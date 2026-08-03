using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.DTOs.Payments;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Api.Controllers;

[Route("api/payments/thawani")]
public class PaymentsController : PublicControllerBase
{
    private readonly IAppDbContext _db;
    private readonly IThawaniPaymentService _thawaniService;

    public PaymentsController(IAppDbContext db, IThawaniPaymentService thawaniService)
    {
        _db = db;
        _thawaniService = thawaniService;
    }

    /// <summary>Creates a Thawani checkout session for a Pending booking and returns the payment URL to redirect to.</summary>
    [HttpPost("initiate/{bookingReference}")]
    public async Task<IActionResult> Initiate(string bookingReference, CancellationToken cancellationToken)
    {
        var booking = await _db.Bookings
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.BookingReference == bookingReference, cancellationToken);

        if (booking is null)
        {
            return NotFound(new { detail = "Booking not found." });
        }

        if (booking.PaymentMethod != PaymentMethod.Thawani)
        {
            return BadRequest(new { detail = "This booking is not set up for online payment." });
        }

        if (booking.Status != BookingStatus.Pending)
        {
            return Conflict(new { detail = "This booking is not awaiting payment." });
        }

        var sessionResult = await _thawaniService.CreateCheckoutSessionAsync(new ThawaniCheckoutRequest
        {
            BookingReference = booking.BookingReference,
            AmountOmr = booking.TotalPrice,
            CustomerName = booking.CustomerName,
            CustomerPhone = booking.CustomerPhone,
            CustomerEmail = booking.CustomerEmail
        }, cancellationToken);

        if (!sessionResult.IsSuccess)
        {
            return HandleFailure(sessionResult);
        }

        if (booking.Payment is not null)
        {
            booking.Payment.ThawaniSessionId = sessionResult.Value!.SessionId;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Ok(new InitiatePaymentResponse { CheckoutUrl = sessionResult.Value!.CheckoutUrl });
    }

    /// <summary>
    /// Called after the customer returns from Thawani's checkout page. We never trust the
    /// redirect alone — this re-verifies the true payment status directly against Thawani's
    /// API before confirming the booking.
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromQuery] string bookingReference, CancellationToken cancellationToken)
    {
        var booking = await _db.Bookings
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.BookingReference == bookingReference, cancellationToken);

        if (booking?.Payment?.ThawaniSessionId is null)
        {
            return NotFound(new { detail = "Booking or payment session not found." });
        }

        var statusResult = await _thawaniService.GetSessionPaymentStatusAsync(booking.Payment.ThawaniSessionId, cancellationToken);
        if (!statusResult.IsSuccess)
        {
            return HandleFailure(statusResult);
        }

        switch (statusResult.Value)
        {
            case "paid":
                booking.Status = BookingStatus.Confirmed;
                booking.Payment.Status = PaymentStatus.Paid;
                booking.Payment.PaidAt = DateTime.UtcNow;
                break;
            case "cancelled":
                booking.Status = BookingStatus.Cancelled;
                booking.Payment.Status = PaymentStatus.Failed;
                var slots = await _db.BookingSlots.Where(s => s.BookingId == booking.Id).ToListAsync(cancellationToken);
                _db.BookingSlots.RemoveRange(slots);
                break;
            // "unpaid" — leave as Pending; customer may still complete payment or it will expire.
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new PaymentStatusResponse
        {
            BookingReference = booking.BookingReference,
            BookingStatus = booking.Status.ToString(),
            PaymentStatus = booking.Payment.Status.ToString()
        });
    }

    /// <summary>Lets the frontend poll for the current status after returning from checkout.</summary>
    [HttpGet("{bookingReference}/status")]
    public async Task<IActionResult> GetStatus(string bookingReference, CancellationToken cancellationToken)
    {
        var booking = await _db.Bookings
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.BookingReference == bookingReference, cancellationToken);

        if (booking is null)
        {
            return NotFound(new { detail = "Booking not found." });
        }

        return Ok(new PaymentStatusResponse
        {
            BookingReference = booking.BookingReference,
            BookingStatus = booking.Status.ToString(),
            PaymentStatus = booking.Payment?.Status.ToString() ?? PaymentStatus.NotRequired.ToString()
        });
    }
}
