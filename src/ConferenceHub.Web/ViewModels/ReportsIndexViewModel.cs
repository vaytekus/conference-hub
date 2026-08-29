using ConferenceHub.Application.DTOs.Reports;

namespace ConferenceHub.Web.ViewModels;

public class ReportsIndexViewModel
{
    public  DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public IReadOnlyList<RoomUtilizationDto>? Utilization { get; set; }
    public RevenueReportDto? Revenue { get; set; }

    public bool HasResults => Utilization is not null && Revenue is not null;
}
