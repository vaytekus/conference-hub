using ConferenceHub.Application.Common;
using ConferenceHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceHub.Infrastructure.Seeders;

public static class IdentitySeeder
{

    public static async Task SeedAsync(IServiceProvider service)
    {
        ArgumentNullException.ThrowIfNull(service);
        var roleManager = service.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = service.GetRequiredService<UserManager<AppUser>>();
        var configuration = service.GetRequiredService<IConfiguration>();

        foreach (var role in new[]{Roles.Admin, Roles.User})
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@ch.local";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin123!";
        var adminUserName = configuration["Seed:AdminUserName"] ?? "admin";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = adminUserName, Email = adminEmail, EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed admin: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
