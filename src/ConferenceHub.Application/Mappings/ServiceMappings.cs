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

    public static Service ToEntity(this CreateServiceDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new Service{Name = dto.Name, Price = dto.Price};
    }

    public static void ApplyTo(this UpdateServiceDto dto, Service service)
    {
        service.Name = dto.Name;
        service.Price = dto.Price;
    }
}
