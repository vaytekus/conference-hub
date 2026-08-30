using ConferenceHub.Application.Common;
using ConferenceHub.Application.DTOs.Auth;
using ConferenceHub.Application.Exceptions;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Options;
using ConferenceHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ConferenceHub.Application.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRepository<RefreshToken> refreshRepo,
    IUnitOfWork uow,
    IOptions<JwtSettings> jwtSettings,
    IOptions<RefreshTokenSettings> refreshTokenSettings) : IAuthService
{
    private const string ReasonReuseDetected = "Reuse detected — token family compromised";
    private const string ReasonRotated = "Rotated";
    private const string ReasonLogout = "Logout";

    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly RefreshTokenSettings _refreshTokenSettings = refreshTokenSettings.Value;

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

        await userManager.AddToRoleAsync(user, Roles.User);
        return await IssueTokensAsync(user, ct);
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

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponseDto?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await refreshRepo.Query()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken, ct);

        if (stored is null)
        {
            return null;
        }

        if (stored.IsRevoked)
        {
            await RevokeAllUserTokensAsync(stored.UserId, ReasonReuseDetected, ct);
            return null;
        }

        if (stored.IsExpired)
        {
            return null;
        }

        var newRefresh = await CreateRefreshTokenAsync(stored.UserId, ct);

        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByToken = newRefresh.Token;
        stored.ReasonRevoked = ReasonRotated;
        refreshRepo.Update(stored);

        await uow.SaveChangesAsync(ct);
        return await BuildResponseAsync(stored.User, newRefresh.Token);
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await refreshRepo.Query()
            .FirstOrDefaultAsync(x => x.Token == refreshToken, ct);

        if (stored is null || !stored.IsActive)
        {
            return false;
        }

        stored.RevokedAt = DateTime.UtcNow;
        stored.ReasonRevoked = ReasonLogout;
        refreshRepo.Update(stored);
        await uow.SaveChangesAsync(ct);

        return true;
    }

    private async Task<AuthResponseDto> IssueTokensAsync(AppUser user, CancellationToken ct)
    {
        var refresh = await CreateRefreshTokenAsync(user.Id, ct);
        await uow.SaveChangesAsync(ct);
        return await BuildResponseAsync(user, refresh.Token);
    }

    private Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var refresh = new RefreshToken
        {
            Token = refreshTokenGenerator.Generate(), UserId = userId, CreatedAt = now, ExpiresAt = now.AddDays(_refreshTokenSettings.ExpiresInDays)
        };

        refreshRepo.Add(refresh);
        return Task.FromResult(refresh);
    }

    private async Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct)
    {
        var active = await refreshRepo.Query()
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var token in active)
        {
            token.RevokedAt = now;
            token.ReasonRevoked = reason;
            refreshRepo.Update(token);
        }

        await uow.SaveChangesAsync(ct);
    }

    private async Task<AuthResponseDto> BuildResponseAsync(AppUser user, string refreshToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenGenerator.GenerateAccessToken(user, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiresInMinutes);

        return new AuthResponseDto(accessToken, refreshToken, expiresAt, user.UserName ?? string.Empty, roles.ToList());
    }
}
