using ConferenceHub.Application.DTOs.Rooms;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Mappings;
using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Application.Services;

public class RoomService(IRepository<Room> repository, IUnitOfWork uow) : IRoomService
{
    public async Task<RoomDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var room = await repository.GetByIdAsync(id, ct);
        return room?.ToDto();
    }

    public async Task<IReadOnlyList<RoomDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rooms = await repository.Query().AsNoTracking().ToListAsync(ct);
        return rooms.Select(r => r.ToDto()).ToList();
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto, CancellationToken ct = default)
    {
        var room = dto.ToEntity();
        repository.Add(room);
        await uow.SaveChangesAsync(ct);
        return room.ToDto();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateRoomDto dto, CancellationToken ct = default)
    {
        var room = await repository.GetByIdAsync(id, ct);
        if (room is null)
        {
            return false;
        }

        dto.ApplyTo(room);
        await uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var room = await repository.GetByIdAsync(id, ct);
        if (room is null)
        {
            return false;
        }

        room.IsDeleted = true;
        await uow.SaveChangesAsync(ct);
        return true;
    }

}
