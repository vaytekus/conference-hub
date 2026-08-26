using ConferenceHub.Application.DTOs.Auth;
using ConferenceHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto, CancellationToken ct)
    {
        var response = await auth.RegisterAsync(dto, ct);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken ct)
    {
        var response = await auth.LoginAsync(dto, ct);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshAsync(RefreshRequestDto dto, CancellationToken ct)
    {
        var response = await auth.RefreshAsync(dto.RefreshToken, ct);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequestDto dto, CancellationToken ct)
    {
        await auth.LogoutAsync(dto.RefreshToken, ct);
        return NoContent();
    }
}
