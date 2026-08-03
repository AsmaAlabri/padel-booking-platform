using PadelBooking.Domain.Common;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// A physical, bookable padel court. Court identity is an admin-only concept —
/// it must never be exposed through customer-facing endpoints or UI (Rule #7).
/// </summary>
public class Court : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Closure> Closures { get; set; } = new List<Closure>();
}
