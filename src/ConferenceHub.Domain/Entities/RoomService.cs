namespace ConferenceHub.Domain.Entities;

public class RoomService
{
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
}
