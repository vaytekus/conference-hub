namespace ConferenceHub.Application.DTOs.Reports;

public record RoomUtilizationDto(
    Guid RoomId,
    string RoomName,
    decimal HoursBooked,
    decimal HoursAvailable,
    decimal UtilizationPercent);
