using ConferenceHub.Application.DTOs.Rooms;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Web.Controllers;

[AllowAnonymous]
public class RoomsController(IRoomService rooms) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(RoomsIndexViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var filter = new RoomSearchDto(model.MinCapacity, model.StartTime, model.EndTime);
        model.Rooms = await rooms.SearchAsync(filter, ct);
        return View(model);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var room = await rooms.GetByIdWithServicesAsync(id, ct);

        if (room is null)
        {
            return NotFound();
        }

        return View(room);
    }
}
