using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(10,3)");

        builder.Property(p => p.ThawaniSessionId)
            .HasMaxLength(200);

        builder.Property(p => p.ThawaniInvoiceId)
            .HasMaxLength(200);

        builder.HasIndex(p => p.BookingId).IsUnique();
    }
}
