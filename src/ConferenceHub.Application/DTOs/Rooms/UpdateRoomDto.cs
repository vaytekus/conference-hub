namespace ConferenceHub.Application.DTOs.Rooms;

public record UpdateRoomDto(string Name, int Capacity, decimal PricePerHour);
