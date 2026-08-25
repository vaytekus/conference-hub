using Microsoft.AspNetCore.Identity;

namespace ConferenceHub.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public ICollection<Reservation> Reservations { get; set; } = [];
}
