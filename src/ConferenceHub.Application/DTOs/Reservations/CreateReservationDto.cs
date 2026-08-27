namespace ConferenceHub.Application.DTOs.Reservations;

public record CreateReservationDto(Guid RoomId, DateTime StartTime, DateTime EndTime, IReadOnlyList<Guid> ServiceIds);
