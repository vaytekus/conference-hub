namespace ConferenceHub.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required decimal TotalPrice { get; set; }
    public required DateTime CreatedAt { get; set; }

    public ICollection<ReservationService> ReservationServices { get; set; } = [];
}
