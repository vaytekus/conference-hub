namespace ConferenceHub.Domain.Entities;

public class Room
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required int Capacity { get; set; }
    public required decimal PricePerHour { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<RoomAmenity> RoomAmenities { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
}
