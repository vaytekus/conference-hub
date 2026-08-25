namespace ConferenceHub.Domain.Entities;

public class Service
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<RoomService> RoomServices { get; set; } = [];
    public ICollection<ReservationService> ReservationServices { get; set; } = [];
}
