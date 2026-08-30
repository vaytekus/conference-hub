using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Options;
using ConferenceHub.Domain.Entities;
using ConferenceHub.Infrastructure.Auth;
using ConferenceHub.Infrastructure.Data;
using ConferenceHub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceHub.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(configuration.GetSection("Identity"));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.Configure<RefreshTokenSettings>(configuration.GetSection(RefreshTokenSettings.SectionName));
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
