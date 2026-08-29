using ConferenceHub.Application.DTOs.Reports;

namespace ConferenceHub.Application.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<RoomUtilizationDto>> GetUtilizationAsync(PeriodQueryDto query, CancellationToken ct = default);
    Task<RevenueReportDto> GetRevenueAsync(PeriodQueryDto query, CancellationToken ct = default);
}
