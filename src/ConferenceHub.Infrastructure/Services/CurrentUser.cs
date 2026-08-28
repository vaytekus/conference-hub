using System.Security.Claims;
using ConferenceHub.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ConferenceHub.Infrastructure.Services;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id
    {
        get
        {
            var raw = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User id claim missing");
            return Guid.Parse(raw);
        }
    }
}
