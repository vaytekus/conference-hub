using System.Data;
using ConferenceHub.Application.DTOs.Reservations;
using ConferenceHub.Application.Exceptions;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Mappings;
using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConferenceHub.Application.Services;

public class BookingService(
    IRepository<Reservation> reservationRepo,
    IRepository<Room> roomRepo,
    IRepository<Service> serviceRepo,
    IUnitOfWork uow,
    IPricingCalculator pricingCalculator,
    ICurrentUser currentUser) : IBookingService
{
    private const int MaxAttempts = 3;

    public async Task<ReservationDto> CreateAsync(CreateReservationDto dto, CancellationToken ct = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await CreateReservationCoreAsync(dto, ct);
            }
            catch (Exception ex) when (IsSerializationFailure(ex) && attempt < MaxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), ct);
            }
        }
    }

    public async Task<IReadOnlyList<ReservationDto>> GetMyReservationsAsync(CancellationToken ct = default)
    {
        var reservations = await reservationRepo.Query()
            .AsNoTracking()
            .Where(r => r.UserId == currentUser.Id)
            .Include(r => r.Room)
            .Include(r => r.ReservationServices)
            .ThenInclude(rs => rs.Service)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(ct);

        return reservations.Select(r => r.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ReservationDto>> GetAllAsync(CancellationToken ct = default)
    {
        var reservations = await reservationRepo.Query()
            .AsNoTracking()
            .Include(r => r.Room)
            .Include(r => r.ReservationServices)
            .ThenInclude(rs => rs.Service)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync(ct);

        return reservations.Select(r => r.ToDto()).ToList();
    }

    private static bool IsSerializationFailure(Exception ex) => ex switch
    {
        PostgresException pg
            => pg.SqlState == PostgresErrorCodes.SerializationFailure,

        DbUpdateConcurrencyException { InnerException: PostgresException pg }
            => pg.SqlState == PostgresErrorCodes.SerializationFailure,

        _ => false
    };

    private async Task<ReservationDto> CreateReservationCoreAsync(
        CreateReservationDto dto,
        CancellationToken ct = default)
    {
        await using var transaction = await uow.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var room = await roomRepo.GetByIdAsync(dto.RoomId, ct);
        if (room is null)
        {
            throw new NotFoundException($"Room {dto.RoomId} not found");
        }

        var services = await serviceRepo.Query()
            .AsNoTracking()
            .Where(s => dto.ServiceIds.Contains(s.Id))
            .ToListAsync(ct);

        if (services.Count != dto.ServiceIds.Count)
        {
            throw new NotFoundException("One or more services not found");
        }

        var hasOverlap = await reservationRepo.Query()
            .AnyAsync(r =>
                r.RoomId == dto.RoomId &&
                r.StartTime < dto.EndTime &&
                r.EndTime > dto.StartTime, ct);

        if (hasOverlap)
        {
            throw new ConflictException("Time slot is already booked for this room");
        }

        var totalPrice = pricingCalculator.Calculate(
            room.PricePerHour,
            dto.StartTime,
            dto.EndTime,
            services.Select(s => s.Price));

        var reservation = new Reservation
        {
            RoomId = dto.RoomId,
            UserId = currentUser.Id,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.UtcNow,
            ReservationServices = services
                .Select(s => new ReservationService
                {
                    ServiceId = s.Id, ServicePriceSnapshot = s.Price
                })
                .ToList()
        };

        reservationRepo.Add(reservation);
        await uow.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        var serviceDtos = services
            .Select(s => new ReservationServiceDto(s.Id, s.Name, s.Price))
            .ToList();

        return new ReservationDto(
            reservation.Id,
            reservation.RoomId,
            room.Name,
            reservation.StartTime,
            reservation.EndTime,
            reservation.TotalPrice,
            reservation.CreatedAt,
            serviceDtos);
    }
}
