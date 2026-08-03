using PadelBooking.Domain.Common;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// A discount campaign applied automatically when a booking matches its criteria.
/// </summary>
public class Offer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    /// <summary>Null = applies to every day of the week within the date range.</summary>
    public DayOfWeek? DayOfWeek { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
