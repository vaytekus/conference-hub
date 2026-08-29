namespace ConferenceHub.Application.DTOs.Reservations;

public record PreviewReservationDto(Guid RoomId, DateTime StartTime, DateTime EndTime, IReadOnlyList<Guid> ServiceIds);
