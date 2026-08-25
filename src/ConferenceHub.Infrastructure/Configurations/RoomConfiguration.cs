using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHub.Infrastructure.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Capacity)
            .IsRequired();

        builder.Property(r => r.PricePerHour)
            .HasColumnType("numeric(18,2)");

        builder.Property(r => r.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasIndex(r => r.Name);
    }
}
