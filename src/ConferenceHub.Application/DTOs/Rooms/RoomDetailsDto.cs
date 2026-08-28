using ConferenceHub.Application.DTOs.Services;

namespace ConferenceHub.Application.DTOs.Rooms;

public record RoomDetailsDto(
    Guid Id,
    string Name,
    int Capacity,
    decimal PricePerHour,
    IReadOnlyList<ServiceDto> Services);
