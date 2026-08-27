namespace ConferenceHub.Application.DTOs.Reservations;

public record ReservationDto(
    Guid Id,
    Guid RoomId,
    string RoomName,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalPrice,
    DateTime CreatedAt,
    IReadOnlyList<ReservationServiceDto> Services);
