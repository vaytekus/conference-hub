using ConferenceHub.Application.Common;
using ConferenceHub.Application.DTOs.Services;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminServicesController(ICatalogService catalog) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await catalog.GetAllServicesAsync(ct);
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ServiceFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var dto = new CreateServiceDto(vm.Name, vm.Price);
        await catalog.CreateAsync(dto, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var service = await catalog.GetByIdAsync(id, ct);
        if (service is null)
        {
            return NotFound();
        }

        var vm = new ServiceFormViewModel
        {
            Id = service.Id, Name = service.Name, Price = service.Price
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ServiceFormViewModel vm, CancellationToken ct)
    {
        if (id != vm.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var dto = new UpdateServiceDto(vm.Name, vm.Price);
        var updated = await catalog.UpdateAsync(id, dto, ct);
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
        var deleted = await catalog.DeleteAsync(id, ct);
        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
