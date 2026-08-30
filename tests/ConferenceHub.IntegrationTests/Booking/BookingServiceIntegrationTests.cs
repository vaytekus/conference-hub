using ConferenceHub.Application.DTOs.Reservations;
using ConferenceHub.Application.Exceptions;
using ConferenceHub.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.IntegrationTests.Booking;

public class BookingServiceIntegrationTests(PostgreSqlFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateAsync_ConcurrentBookings_OnlyOneSucceeds()
    {
        // Arrange
        await using var dbSeed = fixture.CreateDbContext();
        var room = await SeedRoomAsync(dbSeed);

        var start = DateTime.UtcNow.Date.AddDays(10).AddHours(10);
        var end = start.AddHours(2);
        var dto = new CreateReservationDto(room.Id, start, end, []);

        await using var db1 = fixture.CreateDbContext();
        await using var db2 = fixture.CreateDbContext();
        var svc1 = CreateBookingService(db1);
        var svc2 = CreateBookingService(db2);

        // Act
        var task1 = Task.Run(() => svc1.CreateAsync(dto));
        var task2 = Task.Run(() => svc2.CreateAsync(dto));

        await Task.WhenAll(
            task1.ContinueWith(_ => { }),
            task2.ContinueWith(_ => { }));

        // Assert
        await using var verifyDb = fixture.CreateDbContext();
        var count = await verifyDb.Reservations
            .CountAsync(r => r.RoomId == room.Id && r.StartTime == start);
        count.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_BoundaryTouch_BothSucceed()
    {
        await using var db = fixture.CreateDbContext();
        var room = await SeedRoomAsync(db);
        var svc = CreateBookingService(db);

        var baseTime = DateTime.UtcNow.Date.AddDays(11).AddHours(10);

        await svc.CreateAsync(new CreateReservationDto(room.Id, baseTime, baseTime.AddHours(2), []));

        var act = async () => await svc.CreateAsync(
            new CreateReservationDto(room.Id, baseTime.AddHours(2), baseTime.AddHours(4), []));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateAsync_OverlappingSlot_ThrowsConflict()
    {
        await using var db = fixture.CreateDbContext();
        var room = await SeedRoomAsync(db);
        var svc = CreateBookingService(db);

        var baseTime = DateTime.UtcNow.Date.AddDays(12).AddHours(10);

        await svc.CreateAsync(new CreateReservationDto(room.Id, baseTime, baseTime.AddHours(2), []));

        var act = async () => await svc.CreateAsync(
            new CreateReservationDto(room.Id, baseTime.AddHours(1), baseTime.AddHours(3), []));

        await act.Should().ThrowAsync<ConflictException>();
    }
}
