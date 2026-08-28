using ConferenceHub.Application.DTOs.Rooms;

namespace ConferenceHub.Application.Interfaces;

public interface IRoomService
{
    Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RoomDto>> GetAllAsync(CancellationToken ct = default);
    Task<RoomDto> CreateAsync(CreateRoomDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateRoomDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<RoomDto>> SearchAsync(RoomSearchDto filter, CancellationToken ct = default);
    Task<RoomDetailsDto?> GetByIdWithServicesAsync(Guid id, CancellationToken ct = default);
}
