namespace ConferenceHub.Application.DTOs.Rooms;

public record CreateRoomDto(string Name, int Capacity, decimal PricePerHour);
