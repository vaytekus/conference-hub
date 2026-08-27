using ConferenceHub.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ConferenceHub.Api.ExceptionsHandlers;

public class ConflictExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is not ConflictException cex)
        {
            return false;
        }

        context.Response.StatusCode = StatusCodes.Status409Conflict;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.10", title = "Conflict with current resource state", status = 409, detail = cex.Message
        }, ct);

        return true;
    }
}
