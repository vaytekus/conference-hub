using ConferenceHub.Domain.Entities;

namespace ConferenceHub.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(AppUser user, IEnumerable<string> roles);
}
