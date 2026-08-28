using ConferenceHub.Application.DTOs.Services;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Mappings;
using ConferenceHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Application.Services;

public class CatalogService(IRepository<Service> serviceRepo) : ICatalogService
{
    public async Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync(CancellationToken ct = default)
    {
        var services = await serviceRepo.Query()
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return services.Select(s => s.ToDto()).ToList();
    }
}
