using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Options;
using ConferenceHub.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ConferenceHub.Infrastructure.Auth;

public class JwtTokenGenerator(IOptions<JwtSettings> settings) : IJwtTokenGenerator
{
    private readonly JwtSettings _settings = settings.Value;

    public string GenerateAccessToken(AppUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
        issuer: _settings.Issuer,
        audience: _settings.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiresInMinutes),
        signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
