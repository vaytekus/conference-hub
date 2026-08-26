namespace ConferenceHub.Application.DTOs.Auth;

public record AuthResponseDto(string AccessToken, DateTime ExpiresAt, string UserName, IReadOnlyList<string> Roles);
