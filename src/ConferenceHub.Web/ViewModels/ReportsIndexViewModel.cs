using ConferenceHub.Application.DTOs.Reports;

namespace ConferenceHub.Web.ViewModels;

public class ReportsIndexViewModel
{
    private const int DefaultPeriodDays = 30;

    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-DefaultPeriodDays);
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public IReadOnlyList<RoomUtilizationDto>? Utilization { get; set; }
    public RevenueReportDto? Revenue { get; set; }

    public bool HasResults => Utilization is not null && Revenue is not null;
}
