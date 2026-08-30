using ConferenceHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ConferenceHub.Application.Services;

public class TimeZoneProvider(IConfiguration configuration) : ITimeZoneProvider
{
    private readonly TimeZoneInfo _tz = TimeZoneInfo.FindSystemTimeZoneById(
        configuration["SystemTimeZoneId"]
        ?? throw new InvalidOperationException("SystemTimeZoneId is not configured."));

    public TimeZoneInfo Get() => _tz;
}
