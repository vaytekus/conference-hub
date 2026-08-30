using ConferenceHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Web.ViewComponents;

public class ReservationCountViewComponent(IBookingService bookingService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var reservations = await bookingService.GetMyReservationsAsync();
        return View(reservations.Count);
    }
}
