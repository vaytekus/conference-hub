using System.Security.Claims;
using ConferenceHub.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ConferenceHub.Infrastructure.Auth;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

    public Guid Id
    {
        get
        {
            var raw = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User id claim missing");
            return Guid.Parse(raw);
        }
    }
}
