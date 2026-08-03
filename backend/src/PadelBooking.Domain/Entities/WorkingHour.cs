using PadelBooking.Domain.Common;

namespace PadelBooking.Domain.Entities;

/// <summary>
/// Recurring weekly operating hours. One row per day of week.
/// </summary>
public class WorkingHour : BaseEntity
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public bool IsClosed { get; set; }
}
