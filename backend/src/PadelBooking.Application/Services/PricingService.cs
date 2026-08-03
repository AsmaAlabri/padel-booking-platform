using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Application.Services;

public class PricingService : IPricingService
{
    private readonly IAppDbContext _db;

    public PricingService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> GetHourlyPriceAsync(DateOnly date, TimeSpan hourStart, CancellationToken cancellationToken = default)
    {
        var dayOfWeek = date.DayOfWeek;

        var rules = await _db.PriceRules
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);

        // Most specific match wins: a rule scoped to this exact day of week that
        // covers this hour beats the IsDefault fallback rule.
        var specificMatch = rules
            .Where(r => !r.IsDefault
                        && r.DayOfWeek == dayOfWeek
                        && hourStart >= r.StartTime
                        && hourStart < r.EndTime)
            .OrderByDescending(r => r.Id) // most recently created specific rule wins on overlap
            .FirstOrDefault();

        if (specificMatch is not null)
        {
            return specificMatch.PricePerHour;
        }

        var anyDayMatch = rules
            .Where(r => !r.IsDefault
                        && r.DayOfWeek == null
                        && hourStart >= r.StartTime
                        && hourStart < r.EndTime)
            .OrderByDescending(r => r.Id)
            .FirstOrDefault();

        if (anyDayMatch is not null)
        {
            return anyDayMatch.PricePerHour;
        }

        var defaultRule = rules.FirstOrDefault(r => r.IsDefault);
        return defaultRule?.PricePerHour ?? 0m;
    }

    public async Task<Offer?> GetApplicableOfferAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var dayOfWeek = date.DayOfWeek;

        var matchingOffers = await _db.Offers
            .Where(o => o.IsActive
                        && date >= o.StartDate
                        && date <= o.EndDate
                        && (o.DayOfWeek == null || o.DayOfWeek == dayOfWeek))
            .ToListAsync(cancellationToken);

        // Sorted client-side: SQLite's provider can't translate ORDER BY on a
        // decimal column, and the match set here is always small (active offers).
        return matchingOffers
            .OrderByDescending(o => o.DiscountValue) // best offer wins if multiple match
            .FirstOrDefault();
    }
}
