using PadelBooking.Application.DTOs.Availability;

namespace PadelBooking.Application.Interfaces;

public interface IAvailabilityService
{
    /// <summary>
    /// Returns every hourly slot within working hours for the given date, each
    /// marked available/unavailable and priced — with no court information.
    /// </summary>
    Task<DailyAvailabilityDto> GetAvailabilityAsync(DateOnly date, CancellationToken cancellationToken = default);
}
