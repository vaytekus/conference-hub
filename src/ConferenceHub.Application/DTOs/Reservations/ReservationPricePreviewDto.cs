namespace ConferenceHub.Application.DTOs.Reservations;

public record ReservationPricePreviewDto(
    int BillableHours,
    decimal RoomTotal,
    decimal ServicesTotal,
    decimal GrandTotal);
