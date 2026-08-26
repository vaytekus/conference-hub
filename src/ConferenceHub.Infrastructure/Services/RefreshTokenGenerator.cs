using System.Security.Cryptography;
using ConferenceHub.Application.Interfaces;
using ConferenceHub.Application.Options;
using Microsoft.Extensions.Options;

namespace ConferenceHub.Infrastructure.Services;

public class RefreshTokenGenerator(IOptions<RefreshTokenSettings> settings) : IRefreshTokenGenerator
{
    private readonly RefreshTokenSettings _settings = settings.Value;

    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(_settings.TokenSizeBytes);
        return Convert.ToBase64String(bytes);
    }
}
