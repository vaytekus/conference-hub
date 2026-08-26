using ConferenceHub.Application.DTOs.Auth;

namespace ConferenceHub.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default);
}
