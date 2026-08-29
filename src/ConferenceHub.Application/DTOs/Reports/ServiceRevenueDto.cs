namespace ConferenceHub.Application.DTOs.Reports;

public record ServiceRevenueDto(
    Guid ServiceId,
    string ServiceName,
    decimal Total,
    int TimesBooked);
