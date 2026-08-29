namespace ConferenceHub.Application.DTOs.Reports;

public record RevenueReportDto(
    decimal GrandTotal,
    IReadOnlyList<RoomRevenueDto> ByRoom,
    IReadOnlyList<ServiceRevenueDto> ByService);
