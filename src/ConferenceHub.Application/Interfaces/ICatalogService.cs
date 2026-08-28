using ConferenceHub.Application.DTOs.Services;

namespace ConferenceHub.Application.Interfaces;

public interface ICatalogService
{
    Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync(CancellationToken ct = default);
}
