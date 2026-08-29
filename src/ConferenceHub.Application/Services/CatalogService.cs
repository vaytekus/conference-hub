using ConferenceHub.Application.DTOs.Services;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Mappings;
using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Application.Services;

public class CatalogService(
    IRepository<Service> serviceRepo,
    IUnitOfWork uow) : ICatalogService
{
    public async Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync(CancellationToken ct = default)
    {
        var services = await serviceRepo.Query()
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return services.Select(s => s.ToDto()).ToList();
    }

    public async Task<ServiceDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var service = await serviceRepo.GetByIdAsync(id, ct);
        return service?.ToDto();
    }

    public async Task<ServiceDto> CreateAsync(CreateServiceDto dto, CancellationToken ct = default)
    {
        var service = dto.ToEntity();
        serviceRepo.Add(service);
        await uow.SaveChangesAsync(ct);
        return service.ToDto();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateServiceDto dto, CancellationToken ct = default)
    {
        var service = await serviceRepo.GetByIdAsync(id, ct);
        if (service is null)
        {
            return false;
        }

        dto.ApplyTo(service);
        await uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var service = await serviceRepo.GetByIdAsync(id, ct);
        if (service is null)
        {
            return false;
        }

        service.IsDeleted = true;
        await uow.SaveChangesAsync(ct);
        return true;
    }

}
