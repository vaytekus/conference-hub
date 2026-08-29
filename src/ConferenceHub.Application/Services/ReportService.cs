using ConferenceHub.Application.DTOs.Reports;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Application.Services;

public class ReportService(
    IRepository<Reservation> reservationRepo,
    IRepository<Room> roomRepo,
    IPricingCalculator pricingCalculator) : IReportService
{
    private const int OperationHoursPerDay = 17;

    public async Task<IReadOnlyList<RoomUtilizationDto>> GetUtilizationAsync(
        PeriodQueryDto query,
        CancellationToken ct = default)
    {
        var (fromUtc, toUtc, periodDays) = NormalizePeriod(query);

        var reservations = await reservationRepo.Query()
            .AsNoTracking()
            .Where(r => r.StartTime < toUtc && r.EndTime > fromUtc)
            .Select(r => new {r.RoomId, r.StartTime, r.EndTime})
            .ToListAsync(ct);

        var rooms = await roomRepo.Query()
            .AsNoTracking()
            .Select(r => new
            {
                r.Id, r.Name
            })
            .ToListAsync(ct);

        var hoursAvailable = periodDays * OperationHoursPerDay;

        return rooms
            .Select(room => {
                var hoursBooked = reservations
                    .Where(r => r.RoomId == room.Id)
                    .Sum(r => (decimal)BillableHoursInPeriod(r.StartTime, r.EndTime, fromUtc, toUtc));

                var utilizationPercent = hoursAvailable > 0
                    ? Math.Round(hoursBooked / hoursAvailable * 100m, 2)
                    : 0m;

                return new RoomUtilizationDto(room.Id, room.Name, hoursBooked, hoursAvailable, utilizationPercent);
            })
            .OrderByDescending(x => x.UtilizationPercent)
            .ToList();
    }

    public async Task<RevenueReportDto> GetRevenueAsync(
        PeriodQueryDto query,
        CancellationToken ct = default)
    {
        var (fromUtc, toUtc, _) = NormalizePeriod(query);

        var reservations = await reservationRepo.Query()
            .AsNoTracking()
            .Include(x => x.Room)
            .Include(x => x.ReservationServices)
            .ThenInclude(x => x.Service)
            .Where(r => r.StartTime < toUtc && r.EndTime > fromUtc)
            .ToListAsync(ct);

        var grandTotal = reservations.Sum(r => r.TotalPrice);

        var byRoom = reservations
            .GroupBy(x => new
            {
                x.RoomId, x.Room.Name
            })
            .Select(g => new RoomRevenueDto(
                g.Key.RoomId,
                g.Key.Name, g
                .Sum(r => r.TotalPrice)
            ))
            .OrderByDescending(x => x.Total)
            .ToList();

        var byService = reservations
            .SelectMany(r => r.ReservationServices)
            .GroupBy(rs => new
            {
                rs.ServiceId, rs.Service.Name
            })
            .Select(g => new ServiceRevenueDto(
                g.Key.ServiceId,
                g.Key.Name,
                g.Sum(rs => rs.ServicePriceSnapshot),
                g.Count()
            ))
            .OrderByDescending(x => x.Total)
            .ToList();

        return new RevenueReportDto(grandTotal, byRoom, byService);
    }

    private static (DateTime FromUtc, DateTime ToUtc, decimal PeriodDays) NormalizePeriod(PeriodQueryDto query)
    {
        var fromUtc = query.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = query.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var periodDays = (decimal)(toUtc - fromUtc).TotalDays;
        return (fromUtc, toUtc, periodDays);
    }

    private int BillableHoursInPeriod(DateTime resStart, DateTime resEnd, DateTime periodFrom, DateTime periodTo)
    {
        var start = resStart > periodFrom ? resStart : periodFrom;
        var end = resEnd < periodTo ? resEnd : periodTo;
        if (start >= end) return 0;
        return pricingCalculator.CountBillableHours(start, end);
    }

}
