using PadelBooking.Domain.Entities;

namespace PadelBooking.Application.Interfaces;

public interface IPricingService
{
    /// <summary>Hourly price for a given date + hour, using the most specific matching PriceRule.</summary>
    Task<decimal> GetHourlyPriceAsync(DateOnly date, TimeSpan hourStart, CancellationToken cancellationToken = default);

    /// <summary>The best active offer applicable to this date (day-of-week + date range), if any.</summary>
    Task<Offer?> GetApplicableOfferAsync(DateOnly date, CancellationToken cancellationToken = default);
}
