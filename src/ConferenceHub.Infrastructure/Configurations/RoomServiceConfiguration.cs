using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHub.Infrastructure.Configurations;

public class RoomServiceConfiguration : IEntityTypeConfiguration<RoomService>
{
    public void Configure(EntityTypeBuilder<RoomService> builder)
    {
        builder.HasKey(rs => new {rs.RoomId, rs.ServiceId});

        builder.HasQueryFilter(rs => !rs.Room.IsDeleted && !rs.Service.IsDeleted);

        builder.HasOne(rs => rs.Room)
            .WithMany(r => r.RoomServices)
            .HasForeignKey(rs => rs.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.Service)
            .WithMany(r => r.RoomServices)
            .HasForeignKey(rs => rs.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
