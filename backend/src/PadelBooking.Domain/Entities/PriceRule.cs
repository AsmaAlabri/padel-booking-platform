using PadelBooking.Domain.Common;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// Hourly pricing rule. A single "IsDefault" rule with DayOfWeek = null acts as
/// the fallback price; more specific rules (day/time scoped) override it.
/// </summary>
public class PriceRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Null = applies to every day of the week.</summary>
    public DayOfWeek? DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public decimal PricePerHour { get; set; }

    /// <summary>Marks the single fallback rule used when no specific rule matches.</summary>
    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;
}
