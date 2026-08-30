using ConferenceHub.Application.Common;
using ConferenceHub.Application.DTOs.Reports;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class ReportsController(IReportService reports) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? startDate, DateOnly? endDate, CancellationToken ct)
    {
        var vm = new ReportsIndexViewModel();

        if (startDate.HasValue)
        {
            vm.StartDate = startDate.Value;
        }

        if (endDate.HasValue)
        {
            vm.EndDate = endDate.Value;
        }

        if (startDate.HasValue && endDate.HasValue)
        {
            if (vm.StartDate > vm.EndDate)
            {
                ModelState.AddModelError(nameof(vm.StartDate), "Start date must be earlier than or equal to end date.");
                return View(vm);
            }

            var query = new PeriodQueryDto(vm.StartDate, vm.EndDate);
            vm.Utilization = await reports.GetUtilizationAsync(query, ct);
            vm.Revenue = await reports.GetRevenueAsync(query, ct);
        }

        return View(vm);
    }
}
