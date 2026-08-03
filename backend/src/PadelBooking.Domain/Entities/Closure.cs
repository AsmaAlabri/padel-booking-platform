using PadelBooking.Domain.Common;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// A one-off closure (holiday, maintenance, private event). If CourtId is null,
/// the closure applies to ALL courts on that date/time range.
/// </summary>
public class Closure : BaseEntity
{
    public int? CourtId { get; set; }
    public Court? Court { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>Null StartTime/EndTime means the closure covers the full day.</summary>
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    public string? Reason { get; set; }
}
