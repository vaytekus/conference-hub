using ConferenceHub.Application.Common;
using ConferenceHub.Application.DTOs.Reports;
using ConferenceHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class ReportsController(IReportService reports) : ControllerBase
{
    [HttpGet("utilization")]
    public async Task<ActionResult<IReadOnlyList<RoomUtilizationDto>>> GetUtilization(
        [FromQuery] PeriodQueryDto query, CancellationToken ct)
    {
        return Ok(await reports.GetUtilizationAsync(query, ct));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RoomUtilizationDto>> GetRevenue(
        [FromQuery] PeriodQueryDto query, CancellationToken ct)
    {
        return Ok(await reports.GetRevenueAsync(query, ct));
    }
}
