using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHub.Infrastructure.Configurations;

public class RoomAmenityConfiguration : IEntityTypeConfiguration<RoomAmenity>
{
    public void Configure(EntityTypeBuilder<RoomAmenity> builder)
    {
        builder.HasKey(rs => new {rs.RoomId, rs.ServiceId});

        builder.HasQueryFilter(rs => !rs.Room.IsDeleted && !rs.Service.IsDeleted);

        builder.HasOne(rs => rs.Room)
            .WithMany(ra => ra.RoomAmenities)
            .HasForeignKey(rs => rs.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.Service)
            .WithMany(ra => ra.RoomAmenities)
            .HasForeignKey(rs => rs.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
