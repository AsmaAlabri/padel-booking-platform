namespace PadelBooking.Domain.Enums;

/// <summary>
/// Lifecycle states of a booking.
/// </summary>
public enum BookingStatus
{
    /// <summary>Created, awaiting payment confirmation (Thawani) or awaiting arrival (pay on arrival still counts as Confirmed immediately).</summary>
    Pending = 0,

    /// <summary>Court is guaranteed to the customer — either paid online, or accepted as pay-on-arrival.</summary>
    Confirmed = 1,

    /// <summary>Cancelled by customer or admin before the session.</summary>
    Cancelled = 2,

    /// <summary>Session took place.</summary>
    Completed = 3,

    /// <summary>Customer did not show up.</summary>
    NoShow = 4,

    /// <summary>Pending booking that expired because payment was not completed in time.</summary>
    Expired = 5
}
