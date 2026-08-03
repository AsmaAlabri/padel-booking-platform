using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Availability;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IAppDbContext _db;
    private readonly IPricingService _pricingService;

    public AvailabilityService(IAppDbContext db, IPricingService pricingService)
    {
        _db = db;
        _pricingService = pricingService;
    }

    public async Task<DailyAvailabilityDto> GetAvailabilityAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var result = new DailyAvailabilityDto { Date = date };

        // Past dates never have anything bookable (Rule #11).
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (date < today)
        {
            return result;
        }

        var workingHour = await _db.WorkingHours
            .FirstOrDefaultAsync(w => w.DayOfWeek == date.DayOfWeek, cancellationToken);

        if (workingHour is null || workingHour.IsClosed)
        {
            return result; // no slots at all on a closed day
        }

        var activeCourtIds = await _db.Courts
            .Where(c => c.IsActive)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (activeCourtIds.Count == 0)
        {
            return result;
        }

        var closures = await _db.Closures
            .Where(c => c.Date == date)
            .ToListAsync(cancellationToken);

        var bookedByHour = await _db.BookingSlots
            .Where(s => s.SlotDate == date)
            .GroupBy(s => s.SlotHour)
            .Select(g => new { Hour = g.Key, CourtIds = g.Select(s => s.CourtId).ToList() })
            .ToListAsync(cancellationToken);

        var bookedLookup = bookedByHour.ToDictionary(x => x.Hour, x => x.CourtIds);

        var nowTimeOfDay = DateTime.Now.TimeOfDay;

        for (var hour = workingHour.OpenTime; hour < workingHour.CloseTime; hour += TimeSpan.FromHours(1))
        {
            var hourEnd = hour + TimeSpan.FromHours(1);

            // Skip hours that have already started/passed if this is today (Rule #11).
            if (date == today && hour <= nowTimeOfDay)
            {
                result.Slots.Add(new AvailabilitySlotDto
                {
                    StartTime = hour,
                    EndTime = hourEnd,
                    PricePerHour = await _pricingService.GetHourlyPriceAsync(date, hour, cancellationToken),
                    IsAvailable = false
                });
                continue;
            }

            var bookedIds = bookedLookup.TryGetValue(hour, out var ids) ? ids : new List<int>();
            var freeCourtIds = CourtAvailabilityCalculator.GetAvailableCourtIds(
                activeCourtIds, closures, bookedIds, hour, hourEnd);
            var isAvailable = freeCourtIds.Count > 0;

            result.Slots.Add(new AvailabilitySlotDto
            {
                StartTime = hour,
                EndTime = hourEnd,
                PricePerHour = await _pricingService.GetHourlyPriceAsync(date, hour, cancellationToken),
                IsAvailable = isAvailable
            });
        }

        return result;
    }
}
