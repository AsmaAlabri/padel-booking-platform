using PadelBooking.Domain.Entities;

namespace PadelBooking.Application.Common;

/// <summary>
/// Single source of truth for "which courts are free for a given hour" — used by
/// both the read-only availability display and the actual booking creation logic,
/// so the two can never disagree about what's bookable.
/// </summary>
public static class CourtAvailabilityCalculator
{
    public static bool ClosureCoversHour(Closure closure, TimeSpan hourStart, TimeSpan hourEnd)
    {
        // Null Start/EndTime means the closure covers the entire day.
        if (closure.StartTime is null || closure.EndTime is null)
        {
            return true;
        }

        return hourStart < closure.EndTime && hourEnd > closure.StartTime;
    }

    /// <summary>
    /// Returns the subset of activeCourtIds that are free for the given hour —
    /// i.e. not covered by a closure (global or court-specific) and not already booked.
    /// </summary>
    public static List<int> GetAvailableCourtIds(
        IReadOnlyList<int> activeCourtIds,
        IReadOnlyList<Closure> closuresForDate,
        IReadOnlyList<int> bookedCourtIdsForHour,
        TimeSpan hourStart,
        TimeSpan hourEnd)
    {
        var blockedByGlobalClosure = closuresForDate.Any(c => c.CourtId == null && ClosureCoversHour(c, hourStart, hourEnd));
        if (blockedByGlobalClosure)
        {
            return new List<int>();
        }

        var perCourtClosedIds = closuresForDate
            .Where(c => c.CourtId != null && ClosureCoversHour(c, hourStart, hourEnd))
            .Select(c => c.CourtId!.Value)
            .ToHashSet();

        var bookedIds = bookedCourtIdsForHour.ToHashSet();

        return activeCourtIds
            .Where(id => !perCourtClosedIds.Contains(id) && !bookedIds.Contains(id))
            .ToList();
    }
}
