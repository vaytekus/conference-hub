using ConferenceHub.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ConferenceHub.Api.ExceptionsHandlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is not NotFoundException nfex)
        {
            return false;
        }

        context.Response.StatusCode = StatusCodes.Status404NotFound;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.5", title = "Resource not found", status = 404, detail = nfex.Message
        }, ct);

        return true;
    }
}
