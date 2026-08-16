using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Court> Courts => Set<Court>();
    public DbSet<WorkingHour> WorkingHours => Set<WorkingHour>();
    public DbSet<Closure> Closures => Set<Closure>();
    public DbSet<PriceRule> PriceRules => Set<PriceRule>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSlot> BookingSlots => Set<BookingSlot>();
    public DbSet<BookingPlayer> BookingPlayers => Set<BookingPlayer>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public void ClearTrackedEntities() => ChangeTracker.Clear();

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
