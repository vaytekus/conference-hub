using System.Security.Claims;
using ConferenceHub.Application.DTOs.Reservations;
using ConferenceHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController(IBookingService bookings) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationDto dto, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var created = await bookings.CreateAsync(userId, dto, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetMine(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        return Ok(await bookings.GetMyReservationsAsync(userId, ct));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await bookings.GetAllAsync(ct));
    }

    private Guid GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim missing");
        return Guid.Parse(raw);
    }
}
