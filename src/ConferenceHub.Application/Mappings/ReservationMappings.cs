using ConferenceHub.Application.DTOs.Reservations;
using ConferenceHub.Domain.Entities;

namespace ConferenceHub.Application.Mappings;

public static class ReservationMappings
{
    public static ReservationDto ToDto(this Reservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);

        var services = reservation.ReservationServices
            .Select(rs => new ReservationServiceDto(
                rs.ServiceId, rs.Service.Name, rs.ServicePriceSnapshot))
            .ToList();

        return new ReservationDto(
            reservation.Id,
            reservation.RoomId,
            reservation.Room.Name,
            reservation.StartTime,
            reservation.EndTime,
            reservation.TotalPrice,
            reservation.CreatedAt,
            services);
    }
}
