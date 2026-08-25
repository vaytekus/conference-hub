namespace ConferenceHub.Domain.Entities;

public class ReservationService
{
    public Guid ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public decimal ServicePriceSnapshot { get; set; }
}
