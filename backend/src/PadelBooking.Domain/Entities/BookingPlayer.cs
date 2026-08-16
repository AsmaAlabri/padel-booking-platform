using PadelBooking.Domain.Common;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// A teammate the primary customer invites onto their booking. Padel is played
/// in groups (typically 4), so a booking can carry a handful of these — purely
/// informational (no account, no separate contact requirements) so the group
/// shows up together in confirmations and the admin view.
/// </summary>
public class BookingPlayer : BaseEntity
{
    public int BookingId { get; set; }
    public Booking? Booking { get; set; }

    public string Name { get; set; } = string.Empty;
}
