using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHub.Infrastructure.Configurations;

public class ReservationServiceConfiguration : IEntityTypeConfiguration<ReservationService>
{
    public void Configure(EntityTypeBuilder<ReservationService> builder)
    {
        builder.HasKey(rs => new { rs.ReservationId, rs.ServiceId });

        builder.Property(rs => rs.ServicePriceSnapshot).HasColumnType("numeric(18,2)");

        builder.HasOne(rs => rs.Reservation)
            .WithMany(s => s.ReservationServices)
            .HasForeignKey(rs => rs.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rs => rs.Service)
            .WithMany(s => s.ReservationServices)
            .HasForeignKey(rs => rs.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(rs => !rs.Reservation.Room.IsDeleted && !rs.Service.IsDeleted);
    }
}
