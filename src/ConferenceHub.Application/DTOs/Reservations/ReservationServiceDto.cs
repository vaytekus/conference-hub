namespace ConferenceHub.Application.DTOs.Reservations;

public record ReservationServiceDto(Guid ServiceId, string ServiceName, decimal PriceSnapshot);
