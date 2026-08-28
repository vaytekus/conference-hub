using ConferenceHub.Application.DTOs.Services;
using ConferenceHub.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceController(CatalogService catalog) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await catalog.GetAllServicesAsync(ct));
    }
}
