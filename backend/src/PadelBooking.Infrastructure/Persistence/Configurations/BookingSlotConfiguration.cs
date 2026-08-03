using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Infrastructure.Persistence.Configurations;

public class BookingSlotConfiguration : IEntityTypeConfiguration<BookingSlot>
{
    public void Configure(EntityTypeBuilder<BookingSlot> builder)
    {
        builder.ToTable("BookingSlots");

        // The core race-condition guard: the database physically refuses a second
        // row for the same court + date + hour, no matter how two requests interleave.
        builder.HasIndex(s => new { s.CourtId, s.SlotDate, s.SlotHour }).IsUnique();

        builder.HasOne(s => s.Booking)
            .WithMany()
            .HasForeignKey(s => s.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Court)
            .WithMany()
            .HasForeignKey(s => s.CourtId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
