using ConferenceHub.Application.DTOs.Services;

namespace ConferenceHub.Application.Interfaces;

public interface ICatalogService
{
    Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync(CancellationToken ct = default);
    Task<ServiceDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceDto> CreateAsync(CreateServiceDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateServiceDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id,CancellationToken ct = default);

}
