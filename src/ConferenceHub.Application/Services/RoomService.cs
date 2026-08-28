using ConferenceHub.Application.DTOs.Rooms;
using ConferenceHub.Application.DTOs.Services;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Mappings;
using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Application.Services;

public class RoomService(
    IRepository<Room> repository,
    IRepository<Reservation> reservationRepo,
    IUnitOfWork uow) : IRoomService
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

    public async Task<IReadOnlyList<RoomDto>> SearchAsync(RoomSearchDto filter, CancellationToken ct = default)
    {
        if (filter.StartTime.HasValue != filter.EndTime.HasValue)
        {
            return [];
        }

        if (filter.EndTime <= filter.StartTime)
        {
            return [];
        }

        var query = repository.Query().AsNoTracking();

        if (filter.MinCapacity is int min && min > 0)
        {
            query = query.Where(r => r.Capacity >= min);
        }

        if (filter.StartTime.HasValue)
        {
            var start = filter.StartTime!.Value;
            var end = filter.EndTime!.Value;

            query = query.Where(room =>
                !reservationRepo.Query()
                    .Any(res =>
                        res.RoomId == room.Id
                        && res.StartTime < end
                        && res.EndTime > start));
        }

        var rooms = await query.OrderBy(r => r.Name).ToListAsync(ct);
        return rooms.Select(r => r.ToDto()).ToList();
    }

    public async Task<RoomDetailsDto?> GetByIdWithServicesAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var room = await repository.Query()
            .AsNoTracking()
            .Include(r => r.RoomServices).ThenInclude(rs => rs.Service)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (room is null)
        {
            return null;
        }

        var services = room.RoomServices
            .Select(rs => new ServiceDto(rs.Service.Id, rs.Service.Name, rs.Service.Price))
            .ToList();

        return new RoomDetailsDto(
            room.Id,
            room.Name,
            room.Capacity,
            room.PricePerHour,
            services);
    }
}
