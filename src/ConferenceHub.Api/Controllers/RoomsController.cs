using ConferenceHub.Application.Common;
using ConferenceHub.Application.DTOs.Rooms;
using ConferenceHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController(IRoomService rooms): ControllerBase
{
    /// <summary>Get all rooms.</summary>
    /// <returns>List of active (non-deleted) rooms.</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await rooms.GetAllAsync(ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoomDto>> GetById(Guid id, CancellationToken ct)
    {
        var room = await rooms.GetByIdAsync(id, ct);
        return room is null ? NotFound() : Ok(room);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<ActionResult<RoomDto>> Create(CreateRoomDto room, CancellationToken ct)
    {
        var created = await rooms.CreateAsync(room, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRoomDto room, CancellationToken ct)
    {
        return await rooms.UpdateAsync(id, room, ct) ? NoContent() : NotFound();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await rooms.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
