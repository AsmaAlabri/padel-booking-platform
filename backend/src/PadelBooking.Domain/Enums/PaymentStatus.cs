namespace PadelBooking.Domain.Enums;

public enum PaymentStatus
{
    /// <summary>No online payment required yet, or awaiting arrival.</summary>
    NotRequired = 0,

    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}
