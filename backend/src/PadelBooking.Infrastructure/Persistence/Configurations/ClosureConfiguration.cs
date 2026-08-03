using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Infrastructure.Persistence.Configurations;

public class ClosureConfiguration : IEntityTypeConfiguration<Closure>
{
    public void Configure(EntityTypeBuilder<Closure> builder)
    {
        builder.ToTable("Closures");

        builder.Property(c => c.Reason)
            .HasMaxLength(300);

        builder.HasOne(c => c.Court)
            .WithMany(court => court.Closures)
            .HasForeignKey(c => c.CourtId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Date);
    }
}
