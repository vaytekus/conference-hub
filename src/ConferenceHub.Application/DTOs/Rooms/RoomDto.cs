namespace ConferenceHub.Application.DTOs.Rooms;

public record RoomDto(Guid Id, string Name, int Capacity, decimal PricePerHour);
