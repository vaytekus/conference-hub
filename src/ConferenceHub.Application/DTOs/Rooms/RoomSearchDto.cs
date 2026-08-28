namespace ConferenceHub.Application.DTOs.Rooms;

public record RoomSearchDto(int? MinCapacity, DateTime? StartTime, DateTime? EndTime);
