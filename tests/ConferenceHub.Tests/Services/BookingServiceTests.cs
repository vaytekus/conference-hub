using System.Data;
using ConferenceHub.Application.DTOs.Reservations;
using ConferenceHub.Application.Exceptions;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Services;
using ConferenceHub.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable;
using NSubstitute;

namespace ConferenceHub.Tests.Services;

public class BookingServiceTests
{
    private readonly IRepository<Reservation> _reservationRepo = Substitute.For<IRepository<Reservation>>();
    private readonly IRepository<Room> _roomRepo = Substitute.For<IRepository<Room>>();
    private readonly IRepository<Service> _serviceRepo = Substitute.For<IRepository<Service>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IPricingCalculator _pricingCalculator = Substitute.For<IPricingCalculator>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IRetryPolicy _retryPolicy = Substitute.For<IRetryPolicy>();
    private readonly IDbContextTransaction _transaction = Substitute.For<IDbContextTransaction>();

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _roomId = Guid.NewGuid();
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _currentUser.Id.Returns(_userId);
        _uow.BeginTransactionAsync(Arg.Any<IsolationLevel>(), Arg.Any<CancellationToken>())
            .Returns(_transaction);

        _reservationRepo.Query().Returns(new List<Reservation>().BuildMock());
        _serviceRepo.Query().Returns(new List<Service>().BuildMock());

        _retryPolicy
            .ExecuteAsync(Arg.Any<Func<CancellationToken, Task<ReservationDto>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task<ReservationDto>>>()(ci.Arg<CancellationToken>()));

        _sut = new BookingService(
            _reservationRepo, _roomRepo, _serviceRepo,
            _uow, _pricingCalculator, _currentUser, _retryPolicy);
    }

    [Fact]
    public async Task CreateAsync_WhenRoomNotFound_ThrowsNotFoundException()
    {
        var dto = NewDto();
        _roomRepo.GetByIdAsync(dto.RoomId, Arg.Any<CancellationToken>())
            .Returns((Room?)null);

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Room {dto.RoomId} not found");
    }

    [Fact]
    public async Task CreateAsync_WhenServiceIdNotFound_ThrowsNotFoundException()
    {
        var missingServiceId = Guid.NewGuid();
        var dto = NewDto(serviceIds: [missingServiceId]);
        _roomRepo.GetByIdAsync(dto.RoomId, Arg.Any<CancellationToken>())
            .Returns(NewRoom());

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("One or more services not found");
    }

    [Fact]
    public async Task CreateAsync_WhenOverlappingReservationExists_ThrowsConflictException()
    {
        var dto = NewDto(start: Hour(10), end: Hour(12));
        _roomRepo.GetByIdAsync(dto.RoomId, Arg.Any<CancellationToken>())
            .Returns(NewRoom());
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(dto.RoomId, Hour(11), Hour(13))
        }.BuildMock());

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateAsync_WhenBoundaryTouches_DoesNotOverlap()
    {
        var dto = NewDto(start: Hour(12), end: Hour(14));
        _roomRepo.GetByIdAsync(dto.RoomId, Arg.Any<CancellationToken>())
            .Returns(NewRoom());
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(dto.RoomId, Hour(10), Hour(12))
        }.BuildMock());
        _pricingCalculator.Calculate(
                Arg.Any<decimal>(), dto.StartTime, dto.EndTime, Arg.Any<IEnumerable<decimal>>())
            .Returns(200m);

        var result = await _sut.CreateAsync(dto);

        result.TotalPrice.Should().Be(200m);
    }

    [Fact]
    public async Task CreateAsync_WhenValid_SavesReservationAndCommitsTransaction()
    {
        var svcId = Guid.NewGuid();
        var dto = NewDto(serviceIds: [svcId]);
        _roomRepo.GetByIdAsync(dto.RoomId, Arg.Any<CancellationToken>())
            .Returns(NewRoom());
        _serviceRepo.Query().Returns(new List<Service>
        {
            new() { Id = svcId, Name = "Coffee", Price = 10m }
        }.BuildMock());
        _pricingCalculator.Calculate(
                Arg.Any<decimal>(), dto.StartTime, dto.EndTime, Arg.Any<IEnumerable<decimal>>())
            .Returns(210m);

        var result = await _sut.CreateAsync(dto);

        result.RoomId.Should().Be(dto.RoomId);
        result.TotalPrice.Should().Be(210m);
        result.Services.Should().ContainSingle().Which.ServiceName.Should().Be("Coffee");

        _reservationRepo.Received(1).Add(Arg.Is<Reservation>(r =>
            r.RoomId == dto.RoomId
            && r.UserId == _userId
            && r.TotalPrice == 210m
            && r.ReservationServices.Count == 1));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenValid_PassesRoomPriceAndServicePricesToCalculator()
    {
        var svc1Id = Guid.NewGuid();
        var svc2Id = Guid.NewGuid();
        var dto = NewDto(serviceIds: [svc1Id, svc2Id]);
        _roomRepo.GetByIdAsync(dto.RoomId, Arg.Any<CancellationToken>())
            .Returns(NewRoom());
        _serviceRepo.Query().Returns(new List<Service>
        {
            new() { Id = svc1Id, Name = "Coffee", Price = 10m },
            new() { Id = svc2Id, Name = "Projector", Price = 25m }
        }.BuildMock());

        await _sut.CreateAsync(dto);

        _pricingCalculator.Received(1).Calculate(
            100m,
            dto.StartTime,
            dto.EndTime,
            Arg.Is<IEnumerable<decimal>>(p => p.SequenceEqual(new[] { 10m, 25m })));
    }

    [Fact]
    public async Task CreateAsync_WhenValid_StoresServicePriceSnapshots()
    {
        var svcId = Guid.NewGuid();
        var dto = NewDto(serviceIds: [svcId]);
        _roomRepo.GetByIdAsync(dto.RoomId, Arg.Any<CancellationToken>())
            .Returns(NewRoom());
        _serviceRepo.Query().Returns(new List<Service>
        {
            new() { Id = svcId, Name = "Coffee", Price = 15m }
        }.BuildMock());

        await _sut.CreateAsync(dto);

        _reservationRepo.Received(1).Add(Arg.Is<Reservation>(r =>
            r.ReservationServices.Count == 1
            && r.ReservationServices.First().ServiceId == svcId
            && r.ReservationServices.First().ServicePriceSnapshot == 15m));
    }

    [Fact]
    public async Task CreateAsync_WhenOverlapOnDifferentRoom_DoesNotConflict()
    {
        var otherRoomId = Guid.NewGuid();
        var dto = NewDto(start: Hour(10), end: Hour(12));
        _roomRepo.GetByIdAsync(dto.RoomId, Arg.Any<CancellationToken>())
            .Returns(NewRoom());
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(otherRoomId, Hour(10), Hour(12))
        }.BuildMock());
        _pricingCalculator.Calculate(
                Arg.Any<decimal>(), dto.StartTime, dto.EndTime, Arg.Any<IEnumerable<decimal>>())
            .Returns(200m);

        var result = await _sut.CreateAsync(dto);

        result.RoomId.Should().Be(dto.RoomId);
    }

    [Fact]
    public async Task GetMyReservationsAsync_ReturnsOnlyCurrentUserReservations()
    {
        var otherUserId = Guid.NewGuid();
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(_roomId, Hour(10), Hour(11), userId: _userId),
            NewReservation(_roomId, Hour(12), Hour(13), userId: otherUserId),
            NewReservation(_roomId, Hour(14), Hour(15), userId: _userId)
        }.BuildMock());

        var result = await _sut.GetMyReservationsAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllReservationsOrderedByStartTimeDescending()
    {
        var otherUserId = Guid.NewGuid();
        _reservationRepo.Query().Returns(new List<Reservation>
        {
            NewReservation(_roomId, Hour(10), Hour(11), userId: _userId),
            NewReservation(_roomId, Hour(15), Hour(16), userId: otherUserId),
            NewReservation(_roomId, Hour(12), Hour(13), userId: _userId)
        }.BuildMock());

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(3);
        result.Select(r => r.StartTime).Should().BeInDescendingOrder();
    }

    private CreateReservationDto NewDto(
        DateTime? start = null,
        DateTime? end = null,
        IReadOnlyList<Guid>? serviceIds = null)
        => new(
            _roomId,
            start ?? Hour(10),
            end ?? Hour(12),
            serviceIds ?? []);

    private Room NewRoom() => new()
    {
        Id = _roomId,
        Name = "Room A",
        Capacity = 10,
        PricePerHour = 100m
    };

    private Reservation NewReservation(
        Guid roomId,
        DateTime start,
        DateTime end,
        Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        RoomId = roomId,
        UserId = userId ?? _userId,
        StartTime = start,
        EndTime = end,
        TotalPrice = 100m,
        CreatedAt = DateTime.UtcNow,
        Room = new Room
        {
            Id = roomId,
            Name = "Room A",
            Capacity = 10,
            PricePerHour = 100m
        }
    };

    private static DateTime Hour(int h) => new(2026, 9, 1, h, 0, 0, DateTimeKind.Utc);
}
