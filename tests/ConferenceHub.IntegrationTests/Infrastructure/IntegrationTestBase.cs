using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Services;
using ConferenceHub.Domain.Entities;
using ConferenceHub.Infrastructure.Data;
using ConferenceHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SerializationRetryPolicy = ConferenceHub.Application.Services.SerializationRetryPolicy;

namespace ConferenceHub.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase(PostgreSqlFixture fixture)
    : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    protected static readonly Guid TestUserId = Guid.NewGuid();

    protected BookingService CreateBookingService(AppDbContext db)
    {
        var reservationRepo = new Repository<Reservation>(db);
        var roomRepo = new Repository<Room>(db);
        var serviceRepo = new Repository<Service>(db);
        var uow = new UnitOfWork(db);
        var calculator = new PricingCalculator();
        var currentUser = new TestCurrentUser(TestUserId);
        var retryPolicy = new SerializationRetryPolicy();

        return new BookingService(
            reservationRepo, roomRepo, serviceRepo,
            uow, calculator, currentUser, retryPolicy);
    }

    protected async Task<Room> SeedRoomAsync(AppDbContext db, decimal pricePerHour = 1000m)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = $"Test Room {Guid.NewGuid():N}",
            Capacity = 10,
            PricePerHour = pricePerHour
        };
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        return room;
    }

    public async Task InitializeAsync()
    {
        await using var db = fixture.CreateDbContext();

        await db.ReservationServices.ExecuteDeleteAsync();
        await db.Reservations.ExecuteDeleteAsync();

        if (!await db.Users.AnyAsync(u => u.Id == TestUserId))
        {
            db.Users.Add(new AppUser
            {
                Id = TestUserId,
                UserName = "testuser",
                NormalizedUserName = "TESTUSER",
                Email = "test@integration.local",
                NormalizedEmail = "TEST@INTEGRATION.LOCAL",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            });
            await db.SaveChangesAsync();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class TestCurrentUser(Guid userId) : ICurrentUser
{
    public Guid Id => userId;
}