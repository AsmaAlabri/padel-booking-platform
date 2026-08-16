using PadelBooking.Domain.Common;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// A customer's reservation for one or more consecutive hours on a single date.
/// No customer account is required — the booking is looked up later via
/// <see cref="BookingReference"/>. The assigned court is chosen randomly and
/// only at confirmation time (Rule #10), so <see cref="CourtId"/> stays null
/// while the booking is only "being created" inside the transaction.
/// </summary>
public class Booking : BaseEntity
{
    /// <summary>Short, human-shareable code e.g. "PB-7F3K2Q" used for public lookup.</summary>
    public string BookingReference { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }

    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DurationHours { get; set; }

    /// <summary>Randomly assigned court. Never exposed to the customer in API responses.</summary>
    public int CourtId { get; set; }
    public Court? Court { get; set; }

    /// <summary>Price per hour at time of booking (snapshot — protects against later price-rule changes).</summary>
    public decimal PricePerHourSnapshot { get; set; }
    public decimal TotalPrice { get; set; }

    public int? OfferId { get; set; }
    public Offer? Offer { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public Payment? Payment { get; set; }

    /// <summary>Teammates invited onto this booking by the primary customer (optional, informational).</summary>
    public ICollection<BookingPlayer> Players { get; set; } = new List<BookingPlayer>();
}
