using ConferenceHub.Application.DTOs.Reports;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Services;
using ConferenceHub.Domain.Entities;
using FluentAssertions;
using MockQueryable;
using NSubstitute;

namespace ConferenceHub.Tests.Services;

public class ReportServiceTests
{
    private readonly IRepository<Reservation> _reservationRepo = Substitute.For<IRepository<Reservation>>();
    private readonly IRepository<Room> _roomRepo = Substitute.For<IRepository<Room>>();

    private readonly Guid _roomAId = Guid.NewGuid();
    private readonly Guid _roomBId = Guid.NewGuid();
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        _sut = new ReportService(_reservationRepo, _roomRepo, new PricingCalculator());
    }

    // ---------- GetUtilizationAsync ----------

    [Fact]
    public async Task GetUtilizationAsync_WhenNoReservations_ReturnsAllRoomsWithZeroHours()
    {
        _reservationRepo.Query().Returns(new List<Reservation>().BuildMock());
        _roomRepo.Query().Returns(new List<Room>
        {
            NewRoom(_roomAId, "Room A"),
            NewRoom(_roomBId, "Room B")
        }.BuildMock());

        var result = await _sut.GetUtilizationAsync(NewPeriod());

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.HoursBooked == 0m && x.UtilizationPercent == 0m);
    }

    [Fact]
    public async Task GetUtilizationAsync_SumsHoursForSingleRoom()
    {
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(_roomAId, Hour(1, 10), Hour(1, 12)),
            NewReservation(_roomAId, Hour(2, 14), Hour(2, 17))
        }.BuildMock());
        _roomRepo.Query().Returns(new List<Room> { NewRoom(_roomAId, "Room A") }.BuildMock());

        var result = await _sut.GetUtilizationAsync(NewPeriod());

        var roomA = result.Single();
        roomA.HoursBooked.Should().Be(5m);
    }

    [Fact]
    public async Task GetUtilizationAsync_DoesNotMixHoursBetweenRooms()
    {
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(_roomAId, Hour(1, 10), Hour(1, 12)),
            NewReservation(_roomBId, Hour(1, 13), Hour(1, 16))
        }.BuildMock());
        _roomRepo.Query().Returns(new List<Room>
        {
            NewRoom(_roomAId, "Room A"),
            NewRoom(_roomBId, "Room B")
        }.BuildMock());

        var result = await _sut.GetUtilizationAsync(NewPeriod());

        result.Single(x => x.RoomId == _roomAId).HoursBooked.Should().Be(2m);
        result.Single(x => x.RoomId == _roomBId).HoursBooked.Should().Be(3m);
    }

    [Fact]
    public async Task GetUtilizationAsync_ExcludesReservationsOutsidePeriod()
    {
        // Period: Sept 1-3. Reservation on Sept 5 must be excluded.
        var outsideReservation = NewReservation(
            _roomAId,
            new DateTime(2026, 9, 5, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc));

        _reservationRepo.Query().Returns(new List<Reservation> { outsideReservation }.BuildMock());
        _roomRepo.Query().Returns(new List<Room> { NewRoom(_roomAId, "Room A") }.BuildMock());

        var result = await _sut.GetUtilizationAsync(
            new PeriodQueryDto(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3)));

        result.Single().HoursBooked.Should().Be(0m);
    }

    [Fact]
    public async Task GetUtilizationAsync_ComputesPercentAgainstOperatingHours()
    {
        // Period: 2 days → 34 available hours. Booking full day 1 (17h) → 50%.
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(_roomAId, Hour(1, 6), Hour(1, 23))
        }.BuildMock());
        _roomRepo.Query().Returns(new List<Room> { NewRoom(_roomAId, "Room A") }.BuildMock());

        var result = await _sut.GetUtilizationAsync(
            new PeriodQueryDto(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 2)));

        var room = result.Single();
        room.HoursAvailable.Should().Be(34m);
        room.HoursBooked.Should().Be(17m);
        room.UtilizationPercent.Should().Be(50m);
    }

    [Fact]
    public async Task GetUtilizationAsync_MultiDayReservation_ExcludesNighttimeHours()
    {
        // Резервація day1 20:00 → day2 09:00 — 13 wall-clock, але 6 білабельних (3 Evening + 3 Morning).
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(_roomAId, Hour(1, 20), Hour(2, 9))
        }.BuildMock());
        _roomRepo.Query().Returns(new List<Room> { NewRoom(_roomAId, "Room A") }.BuildMock());

        var result = await _sut.GetUtilizationAsync(NewPeriod());

        result.Single().HoursBooked.Should().Be(6m);
    }

    [Fact]
    public async Task GetUtilizationAsync_ReservationClippedToPeriod()
    {
        // Резервація Sept 1 10:00 → Sept 3 14:00. Період Sept 2 → 2 повних дні clip'у.
        // Clipped range: Sept 2 00:00 → Sept 3 00:00 → billable = 17h (6..23).
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(_roomAId, Hour(1, 10), Hour(3, 14))
        }.BuildMock());
        _roomRepo.Query().Returns(new List<Room> { NewRoom(_roomAId, "Room A") }.BuildMock());

        var result = await _sut.GetUtilizationAsync(
            new PeriodQueryDto(new DateOnly(2026, 9, 2), new DateOnly(2026, 9, 2)));

        result.Single().HoursBooked.Should().Be(17m);
    }

    [Fact]
    public async Task GetUtilizationAsync_OrdersRoomsByPercentDescending()
    {
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(_roomAId, Hour(1, 10), Hour(1, 12)),  // 2h in A
            NewReservation(_roomBId, Hour(1, 10), Hour(1, 15))   // 5h in B
        }.BuildMock());
        _roomRepo.Query().Returns(new List<Room>
        {
            NewRoom(_roomAId, "Room A"),
            NewRoom(_roomBId, "Room B")
        }.BuildMock());

        var result = await _sut.GetUtilizationAsync(NewPeriod());

        result[0].RoomId.Should().Be(_roomBId);
        result[1].RoomId.Should().Be(_roomAId);
    }

    // ---------- GetRevenueAsync ----------

    [Fact]
    public async Task GetRevenueAsync_WhenNoReservations_ReturnsZeros()
    {
        _reservationRepo.Query().Returns(new List<Reservation>().BuildMock());

        var result = await _sut.GetRevenueAsync(NewPeriod());

        result.GrandTotal.Should().Be(0m);
        result.ByRoom.Should().BeEmpty();
        result.ByService.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRevenueAsync_SumsGrandTotalAndGroupsByRoom()
    {
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(_roomAId, Hour(1, 10), Hour(1, 12), totalPrice: 200m, roomName: "Room A"),
            NewReservation(_roomAId, Hour(2, 10), Hour(2, 12), totalPrice: 150m, roomName: "Room A"),
            NewReservation(_roomBId, Hour(1, 13), Hour(1, 15), totalPrice: 300m, roomName: "Room B")
        }.BuildMock());

        var result = await _sut.GetRevenueAsync(NewPeriod());

        result.GrandTotal.Should().Be(650m);
        result.ByRoom.Should().HaveCount(2);
        result.ByRoom.Single(x => x.RoomId == _roomAId).Total.Should().Be(350m);
        result.ByRoom.Single(x => x.RoomId == _roomBId).Total.Should().Be(300m);
    }

    [Fact]
    public async Task GetRevenueAsync_AggregatesServicesWithSnapshotPricesAndCounts()
    {
        var coffeeId = Guid.NewGuid();
        var projectorId = Guid.NewGuid();

        var r1 = NewReservation(_roomAId, Hour(1, 10), Hour(1, 12), totalPrice: 100m, roomName: "Room A");
        r1.ReservationServices = new List<ReservationService>
        {
            NewReservationService(coffeeId, "Coffee", snapshotPrice: 10m),
            NewReservationService(projectorId, "Projector", snapshotPrice: 25m)
        };

        var r2 = NewReservation(_roomBId, Hour(2, 10), Hour(2, 12), totalPrice: 100m, roomName: "Room B");
        r2.ReservationServices = new List<ReservationService>
        {
            NewReservationService(coffeeId, "Coffee", snapshotPrice: 15m)  // snapshot differs — historical price
        };

        _reservationRepo.Query().Returns(new List<Reservation> { r1, r2 }.BuildMock());

        var result = await _sut.GetRevenueAsync(NewPeriod());

        var coffee = result.ByService.Single(x => x.ServiceId == coffeeId);
        coffee.Total.Should().Be(25m);        // 10 + 15 snapshots
        coffee.TimesBooked.Should().Be(2);

        var projector = result.ByService.Single(x => x.ServiceId == projectorId);
        projector.Total.Should().Be(25m);
        projector.TimesBooked.Should().Be(1);
    }

    [Fact]
    public async Task GetRevenueAsync_IncludesReservationStartingBeforePeriodButOverlapping()
    {
        // Стара реалізація пропускала резервацію що почалась до `from` — тепер overlap-фільтр включає її.
        var overlapping = NewReservation(
            _roomAId,
            new DateTime(2026, 8, 31, 20, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc),
            totalPrice: 500m,
            roomName: "Room A");

        _reservationRepo.Query().Returns(new List<Reservation> { overlapping }.BuildMock());

        var result = await _sut.GetRevenueAsync(NewPeriod());

        result.GrandTotal.Should().Be(500m);
    }

    [Fact]
    public async Task GetRevenueAsync_ExcludesReservationsOutsidePeriod()
    {
        var outsideReservation = NewReservation(
            _roomAId,
            new DateTime(2026, 9, 5, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc),
            totalPrice: 999m,
            roomName: "Room A");

        _reservationRepo.Query().Returns(new List<Reservation> { outsideReservation }.BuildMock());

        var result = await _sut.GetRevenueAsync(
            new PeriodQueryDto(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3)));

        result.GrandTotal.Should().Be(0m);
        result.ByRoom.Should().BeEmpty();
    }

    // ---------- Helpers ----------

    private static PeriodQueryDto NewPeriod() =>
        new(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3));

    private static Room NewRoom(Guid id, string name) => new()
    {
        Id = id,
        Name = name,
        Capacity = 10,
        PricePerHour = 100m
    };

    private static Reservation NewReservation(
        Guid roomId,
        DateTime start,
        DateTime end,
        decimal totalPrice = 100m,
        string roomName = "Room") => new()
    {
        Id = Guid.NewGuid(),
        RoomId = roomId,
        UserId = Guid.NewGuid(),
        StartTime = start,
        EndTime = end,
        TotalPrice = totalPrice,
        CreatedAt = DateTime.UtcNow,
        Room = new Room
        {
            Id = roomId,
            Name = roomName,
            Capacity = 10,
            PricePerHour = 100m
        },
        ReservationServices = new List<ReservationService>()
    };

    private static ReservationService NewReservationService(
        Guid serviceId, string serviceName, decimal snapshotPrice) => new()
    {
        ServiceId = serviceId,
        ServicePriceSnapshot = snapshotPrice,
        Service = new Service
        {
            Id = serviceId,
            Name = serviceName,
            Price = snapshotPrice
        }
    };

    private static DateTime Hour(int day, int hour) =>
        new(2026, 9, day, hour, 0, 0, DateTimeKind.Utc);
}
