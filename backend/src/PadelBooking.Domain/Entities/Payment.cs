using PadelBooking.Domain.Common;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// Payment record for a booking. For PayOnArrival bookings this row still exists
/// (Status = NotRequired/Pending) for a consistent audit trail. For Thawani,
/// this holds the sandbox session/invoice identifiers used to verify the callback.
/// </summary>
public class Payment : BaseEntity
{
    public int BookingId { get; set; }
    public Booking? Booking { get; set; }

    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? ThawaniSessionId { get; set; }
    public string? ThawaniInvoiceId { get; set; }

    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
}
