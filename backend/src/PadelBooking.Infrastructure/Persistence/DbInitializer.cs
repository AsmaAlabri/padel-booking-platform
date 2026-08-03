using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Entities;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Infrastructure.Persistence;

/// <summary>
/// Seeds the minimum data needed for the app to be usable out of the box:
/// one admin login, three courts, a full week of working hours, and one
/// default price rule. Idempotent — safe to call on every startup.
/// </summary>
public static class DbInitializer
{
    public const string DefaultAdminUsername = "admin";
    public const string DefaultAdminPassword = "Admin@123";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher passwordHasher)
    {
        await db.Database.MigrateAsync();

        if (!await db.AdminUsers.AnyAsync())
        {
            db.AdminUsers.Add(new AdminUser
            {
                Username = DefaultAdminUsername,
                PasswordHash = passwordHasher.HashPassword(DefaultAdminPassword),
                Role = AdminRole.SuperAdmin,
                IsActive = true
            });
        }

        if (!await db.Courts.AnyAsync())
        {
            db.Courts.AddRange(
                new Court { Name = "Court 1", IsActive = true },
                new Court { Name = "Court 2", IsActive = true },
                new Court { Name = "Court 3", IsActive = true }
            );
        }

        if (!await db.WorkingHours.AnyAsync())
        {
            foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            {
                db.WorkingHours.Add(new WorkingHour
                {
                    DayOfWeek = day,
                    OpenTime = new TimeSpan(8, 0, 0),
                    CloseTime = new TimeSpan(23, 0, 0),
                    IsClosed = false
                });
            }
        }

        if (!await db.PriceRules.AnyAsync())
        {
            db.PriceRules.Add(new PriceRule
            {
                Name = "Standard Rate",
                DayOfWeek = null,
                StartTime = new TimeSpan(0, 0, 0),
                EndTime = new TimeSpan(23, 59, 59),
                PricePerHour = 6.000m,
                IsDefault = true,
                IsActive = true
            });
        }

        await db.SaveChangesAsync();
    }
}
