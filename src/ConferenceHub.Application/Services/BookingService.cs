using System.Data;
using ConferenceHub.Application.DTOs.Reservations;
using ConferenceHub.Application.Exceptions;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Mappings;
using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Application.Services;

public class BookingService(
    IRepository<Reservation> reservationRepo,
    IRepository<Room> roomRepo,
    IRepository<Service> serviceRepo,
    IUnitOfWork uow,
    IPricingCalculator pricingCalculator,
    ICurrentUser currentUser,
    IRetryPolicy retryPolicy,
    ITimeZoneProvider timeZoneProvider) : IBookingService
{
    public Task<ReservationDto> CreateAsync(CreateReservationDto dto, CancellationToken ct = default)
        => retryPolicy.ExecuteAsync(c => CreateReservationCoreAsync(dto, c), ct);

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

        var tz = timeZoneProvider.Get();

        return reservations.Select(r => ToLocalDto(r, tz)).ToList();
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

        var tz = timeZoneProvider.Get();

        return reservations.Select(r => ToLocalDto(r, tz)).ToList();
    }

    public async Task<ReservationPricePreviewDto> PreviewPriceAsync(
        PreviewReservationDto dto,
        CancellationToken ct = default)
    {
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

        var billableHours = pricingCalculator.CountBillableHours(dto.StartTime, dto.EndTime);
        var roomTotal = pricingCalculator.Calculate(
            room.PricePerHour,
            dto.StartTime,
            dto.EndTime,
            Array.Empty<decimal>());

        var serviceTotal = services.Sum(s => s.Price);
        var grandTotal = roomTotal + serviceTotal;

        return new ReservationPricePreviewDto(billableHours, roomTotal, serviceTotal, grandTotal);
    }

    public async Task<IReadOnlyList<RoomSlotDto>> GetRoomAvailabilityAsync(
        Guid roomId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
    {
        var tz = timeZoneProvider.Get();
        var rangeStartUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified), tz);
        var rangeEndUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(to.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Unspecified), tz);

        var slots = await reservationRepo.Query()
            .AsNoTracking()
            .Where(r => r.RoomId == roomId
                && r.StartTime < rangeEndUtc
                && r.EndTime > rangeStartUtc)
            .OrderBy(r => r.StartTime)
            .Select(r => new { r.StartTime, r.EndTime })
            .ToListAsync(ct);

        return slots
            .Select(s => new RoomSlotDto(
                TimeZoneInfo.ConvertTimeFromUtc(s.StartTime, tz),
                TimeZoneInfo.ConvertTimeFromUtc(s.EndTime, tz)))
            .ToList();
    }

    private async Task<ReservationDto> CreateReservationCoreAsync(
        CreateReservationDto dto,
        CancellationToken ct = default)
    {
        var tz = timeZoneProvider.Get();
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Unspecified), tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Unspecified), tz);

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
                r.StartTime < endUtc &&
                r.EndTime > startUtc, ct);

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
            StartTime = startUtc,
            EndTime = endUtc,
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
            dto.StartTime,
            dto.EndTime,
            reservation.TotalPrice,
            reservation.CreatedAt,
            serviceDtos);
    }

    private static ReservationDto ToLocalDto(Reservation r, TimeZoneInfo tz) =>
        r.ToDto() with
        {
            StartTime = TimeZoneInfo.ConvertTimeFromUtc(r.StartTime, tz),
            EndTime   = TimeZoneInfo.ConvertTimeFromUtc(r.EndTime, tz)
        };
}
