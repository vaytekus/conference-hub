using System.Reflection;
using ConferenceHub.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ConferenceHub.Application.Services;
using FluentValidation;

namespace ConferenceHub.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddSingleton<IPricingCalculator, PricingCalculator>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
