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
        var created = await bookings.CreateAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPost("total-price")]
    public async Task<ActionResult<ReservationPricePreviewDto>> Preview(
        PreviewReservationDto dto, CancellationToken ct)
    {
        return Ok(await bookings.PreviewPriceAsync(dto, ct));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetMine(CancellationToken ct)
    {
        return Ok(await bookings.GetMyReservationsAsync(ct));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await bookings.GetAllAsync(ct));
    }
}
