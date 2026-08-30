using ConferenceHub.Application.DTOs.Reservations;
using ConferenceHub.Application.Exceptions;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Web.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Web.Controllers;

[Authorize]
public class ReservationsController(
    IRoomService roomService,
    IBookingService bookingService,
    IValidator<PreviewReservationDto> previewValidator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(Guid roomId, CancellationToken ct)
    {
        var room = await roomService.GetByIdWithServicesAsync(roomId, ct);
        if (room is null)
        {
            return NotFound();
        }

        var vm = new BookReservationViewModel
        {
            RoomId = room.Id,
            RoomName = room.Name,
            Capacity = room.Capacity,
            PricePerHour = room.PricePerHour,
            AvailableServices = room.Services
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookReservationViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateAsync(model, ct);
            return View(model);
        }

        try
        {
            var dto = new CreateReservationDto(
                model.RoomId,
                model.StartTime!.Value,
                model.EndTime!.Value,
                model.SelectedServiceIds);

            await bookingService.CreateAsync(dto, ct);
            return RedirectToAction(nameof(Mine));
        }
        catch (Exception ex) when (ex is ConflictException or NotFoundException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await RepopulateAsync(model, ct);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var reservation = await bookingService.GetMyReservationsAsync(ct);
        return View(reservation);
    }

    [HttpPost]
    public async Task<IActionResult> Preview(
        [FromBody] PreviewReservationDto dto,
        CancellationToken ct)
    {
        var validation = await previewValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            return BadRequest(new {errors = validation.Errors.Select(x => x.ErrorMessage)});
        }

        try
        {
            var price = await bookingService.PreviewPriceAsync(dto, ct);
            return Json(price);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private async Task RepopulateAsync(BookReservationViewModel model, CancellationToken ct)
    {
        var room = await roomService.GetByIdWithServicesAsync(model.RoomId, ct);

        if (room is null)
        {
            return;
        }

        model.RoomName = room.Name;
        model.Capacity = room.Capacity;
        model.PricePerHour = room.PricePerHour;
        model.AvailableServices = room.Services;
    }
}
