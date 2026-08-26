using ConferenceHub.Application.DTOs.Auth;
using ConferenceHub.Application.Exceptions;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Options;
using ConferenceHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ConferenceHub.Application.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    IJwtTokenGenerator tokenGenerator,
    IOptions<JwtSettings> jwtSettings) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var user = new AppUser
        {
            UserName = dto.UserName, Email = dto.Email,
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(x => x.Description));
        }

        await userManager.AddToRoleAsync(user, "User");
        return await BuildResponseAsync(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return null;
        }

        var valid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!valid)
        {
            return null;
        }

        return await BuildResponseAsync(user);
    }

    private async Task<AuthResponseDto> BuildResponseAsync(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var token = tokenGenerator.GenerateAccessToken(user, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiresInMinutes);

        return new AuthResponseDto(token, expiresAt, user.UserName ?? string.Empty, roles.ToList());
    }
}
