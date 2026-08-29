using ConferenceHub.Application.DTOs.Services;
using ConferenceHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(ICatalogService catalog) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await catalog.GetAllServicesAsync(ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceDto>> GetById(Guid id, CancellationToken ct)
    {
        var service = await catalog.GetByIdAsync(id, ct);
        return service is null ? NotFound() : Ok(service);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create(CreateServiceDto dto, CancellationToken ct)
    {
        var created = await catalog.CreateAsync(dto, ct);
        return  CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateServiceDto dto, CancellationToken ct)
    {
        return await catalog.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await catalog.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
