using ConferenceHub.Application.DTOs.Reservations;

namespace ConferenceHub.Application.Interfaces;

public interface IBookingService
{
    Task<ReservationDto> CreateAsync(CreateReservationDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ReservationDto>> GetMyReservationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReservationDto>> GetAllAsync(CancellationToken ct = default);

    Task<ReservationPricePreviewDto> PreviewPriceAsync(PreviewReservationDto dto, CancellationToken ct = default);
}
