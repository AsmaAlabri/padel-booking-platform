using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Infrastructure.Persistence.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("AdminUsers");

        builder.Property(a => a.Username)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(a => a.Username).IsUnique();

        builder.Property(a => a.PasswordHash)
            .IsRequired();
    }
}
