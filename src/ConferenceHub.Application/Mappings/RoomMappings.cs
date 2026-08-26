using ConferenceHub.Application.DTOs.Rooms;
using ConferenceHub.Domain.Entities;

namespace ConferenceHub.Application.Mappings;

public static class RoomMappings
{
    public static RoomDto ToDto(this Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        return new(room.Id, room.Name, room.Capacity, room.PricePerHour);
    }

    public static Room ToEntity(this CreateRoomDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new()
        {
            Name = dto.Name, Capacity = dto.Capacity, PricePerHour = dto.PricePerHour
        };
    }

    public static void ApplyTo(this UpdateRoomDto dto, Room room)
    {
        room.Name = dto.Name;
        room.Capacity = dto.Capacity;
        room.PricePerHour = dto.PricePerHour;
    }
}
