using ConferenceHub.Application.DTOs.Services;
using ConferenceHub.Domain.Entities;

namespace ConferenceHub.Application.Mappings;

public static class ServiceMappings
{
    public static ServiceDto ToDto(this Service service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return new ServiceDto(service.Id, service.Name, service.Price);
    }
}
