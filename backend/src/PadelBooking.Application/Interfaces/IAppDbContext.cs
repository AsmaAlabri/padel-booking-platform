using Microsoft.EntityFrameworkCore;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Application.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext so Application-layer services depend on
/// this interface (Domain-adjacent) rather than the concrete Infrastructure DbContext,
/// keeping the dependency direction Api -> Infrastructure -> Application -> Domain.
/// </summary>
public interface IAppDbContext
{
    DbSet<Court> Courts { get; }
    DbSet<WorkingHour> WorkingHours { get; }
    DbSet<Closure> Closures { get; }
    DbSet<PriceRule> PriceRules { get; }
    DbSet<Offer> Offers { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<BookingSlot> BookingSlots { get; }
    DbSet<BookingPlayer> BookingPlayers { get; }
    DbSet<Payment> Payments { get; }
    DbSet<AdminUser> AdminUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Detaches all currently tracked entities. Used when a save attempt fails due
    /// to a unique-constraint conflict (another request grabbed the same court-hour)
    /// so the next retry starts from a clean slate instead of re-submitting the same
    /// failed inserts.
    /// </summary>
    void ClearTrackedEntities();
}
