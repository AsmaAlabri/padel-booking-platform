using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.Property(b => b.BookingReference)
            .IsRequired()
            .HasMaxLength(20);
        builder.HasIndex(b => b.BookingReference).IsUnique();

        builder.Property(b => b.CustomerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.CustomerPhone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(b => b.CustomerEmail)
            .HasMaxLength(200);

        builder.Property(b => b.PricePerHourSnapshot)
            .HasColumnType("decimal(10,3)");

        builder.Property(b => b.TotalPrice)
            .HasColumnType("decimal(10,3)");

        builder.HasOne(b => b.Court)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CourtId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Offer)
            .WithMany(o => o.Bookings)
            .HasForeignKey(b => b.OfferId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.Payment)
            .WithOne(p => p.Booking)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Speeds up availability lookups: "find bookings for court X on date Y overlapping a time range"
        builder.HasIndex(b => new { b.BookingDate, b.CourtId, b.Status });
    }
}
