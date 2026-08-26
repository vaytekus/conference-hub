namespace ConferenceHub.Application.DTOs.Auth;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    string UserName,
    IReadOnlyList<string> Roles);
