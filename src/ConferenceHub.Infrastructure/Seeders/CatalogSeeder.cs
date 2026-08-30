using Bogus;
using ConferenceHub.Domain.Entities;
using ConferenceHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Infrastructure.Seeders;

public static class CatalogSeeder
{
    private const int RoomsCount = 8;
    private const string Locale = "en";
    private const int RandomSeed = 42;

    private static readonly (string Name, decimal Price)[] SeedServices =
    [
        ("Projector", 150m),
        ("Whiteboard", 50m),
        ("Video conferencing", 300m),
        ("Coffee break", 200m),
        ("Flipchart", 40m),
        ("Sound system", 250m)
    ];

    private static readonly string[] RoomNames =
    [
        "Alpha", "Beta", "Gamma", "Delta", "Omega",
        "Nova", "Orion", "Vega", "Lyra", "Atlas"
    ];

    public static async Task SeedAsync(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        Randomizer.Seed = new Random(RandomSeed);

        await SeedServicesAsync(db);
        await SeedRoomsAsync(db);
    }

    private static async Task SeedServicesAsync(AppDbContext db)
    {
        if (await db.Services.AnyAsync())
        {
            return;
        }

        var services = SeedServices.Select(s => new Service
        {
            Id = Guid.NewGuid(), Name = s.Name, Price = s.Price
        });

        db.Services.AddRange(services);
        await db.SaveChangesAsync();
    }

    private static async Task SeedRoomsAsync(AppDbContext db)
    {
        if (await db.Rooms.AnyAsync())
        {
            return;
        }

        var services = await db.Services.ToListAsync();
        var picker = new Faker();

        var roomFaker = new Faker<Room>(Locale)
            .RuleFor(r => r.Id, _ => Guid.NewGuid())
            .RuleFor(r => r.Name, f => $"Room {f.PickRandom(RoomNames)}-{f.Random.Number(100, 999)}")
            .RuleFor(r => r.Capacity, f => f.Random.Int(4, 40))
            .RuleFor(r => r.PricePerHour, f => Math.Round(f.Random.Decimal(200m, 1500m), 2));

        var rooms = roomFaker.Generate(RoomsCount);

        foreach (var room in rooms)
        {
            var picked = picker.PickRandom(services, picker.Random.Int(1, 4)).ToList();
            foreach (var svc in picked)
            {
                room.RoomAmenities.Add(new RoomAmenity { ServiceId = svc.Id });
            }
        }

        db.Rooms.AddRange(rooms);
        await db.SaveChangesAsync();
    }
}
