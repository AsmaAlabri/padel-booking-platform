using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Infrastructure.Persistence.Configurations;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("Offers");

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(o => o.Description)
            .HasMaxLength(500);

        builder.Property(o => o.DiscountValue)
            .HasColumnType("decimal(10,3)");
    }
}
