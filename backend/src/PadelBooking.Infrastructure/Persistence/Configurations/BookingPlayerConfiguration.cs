using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Infrastructure.Persistence.Configurations;

public class BookingPlayerConfiguration : IEntityTypeConfiguration<BookingPlayer>
{
    public void Configure(EntityTypeBuilder<BookingPlayer> builder)
    {
        builder.ToTable("BookingPlayers");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(p => p.Booking)
            .WithMany(b => b.Players)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
