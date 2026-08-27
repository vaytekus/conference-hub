using ConferenceHub.Application.DTOs.Reservations;

namespace ConferenceHub.Application.Interfaces;

public interface IBookingService
{
    Task<ReservationDto> CreateAsync(Guid userId, CreateReservationDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ReservationDto>> GetMyReservationsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ReservationDto>> GetAllAsync(CancellationToken ct = default);
}
