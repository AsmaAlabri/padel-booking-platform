using PadelBooking.Domain.Common;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// Materialized record of a single booked hour on a single court. Exists purely
/// to let the database itself enforce "one court can't be double-booked for the
/// same hour" via a unique index on (CourtId, SlotDate, SlotHour) — this makes
/// the guarantee hold regardless of transaction isolation level, and identically
/// on both SQLite (dev) and SQL Server (production).
///
/// One Booking with DurationHours = 3 produces 3 BookingSlot rows. Cancelling
/// the booking deletes its slots, freeing the hours back up immediately.
/// </summary>
public class BookingSlot : BaseEntity
{
    public int BookingId { get; set; }
    public Booking? Booking { get; set; }

    public int CourtId { get; set; }
    public Court? Court { get; set; }

    public DateOnly SlotDate { get; set; }

    /// <summary>The start of the booked hour, e.g. 18:00:00 for the 18:00-19:00 slot.</summary>
    public TimeSpan SlotHour { get; set; }
}
