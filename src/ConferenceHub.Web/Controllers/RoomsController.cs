using ConferenceHub.Application.DTOs.Rooms;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Web.Controllers;

[AllowAnonymous]
public class RoomsController(IRoomService rooms, IBookingService booking) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(RoomsIndexViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var filter = new RoomSearchDto(model.MinCapacity, model.StartTime, model.EndTime);
        var allRooms = await rooms.SearchAsync(filter, ct);

        model.TotalCount = allRooms.Count;
        model.Rooms = allRooms
            .Skip((model.Page - 1) * model.GetPageSize())
            .Take(model.GetPageSize())
            .ToList();

        return View(model);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, DateOnly? date, DateOnly? dateTo, CancellationToken ct)
    {
        var room = await rooms.GetByIdWithServicesAsync(id, ct);
        if (room is null)
        {
            return NotFound();
        }

        var from = date ?? DateOnly.FromDateTime(DateTime.Today);
        var to = dateTo ?? from;
        if (to < from) to = from;

        var slots = await booking.GetRoomAvailabilityAsync(id, from, to, ct);

        ViewBag.AvailabilityFrom = from;
        ViewBag.AvailabilityTo = to;
        ViewBag.BookedSlots = slots;

        return View(room);
    }
}
