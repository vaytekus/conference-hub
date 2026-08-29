using ConferenceHub.Application.DTOs.Rooms;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminRoomsController(IRoomService rooms) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await rooms.GetAllAsync(ct);
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new RoomFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var dto = new CreateRoomDto(vm.Name, vm.Capacity, vm.PricePerHour);
        await rooms.CreateAsync(dto, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var room = await rooms.GetByIdAsync(id, ct);
        if (room is null)
        {
            return NotFound();
        }

        var vm = new RoomFormViewModel
        {
            Id = room.Id, Name = room.Name, Capacity = room.Capacity, PricePerHour = room.PricePerHour
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RoomFormViewModel vm, CancellationToken ct)
    {
        if (vm.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var dto = new UpdateRoomDto(vm.Name, vm.Capacity, vm.PricePerHour);
        var updated = await rooms.UpdateAsync(id, dto, ct);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await rooms.DeleteAsync(id, ct);
        if(!deleted)
        {
            return NotFound();
        }
        return RedirectToAction(nameof(Index));
    }
}
