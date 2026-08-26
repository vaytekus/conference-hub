using ConferenceHub.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ConferenceHub.Api.ExceptionsHandlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not ValidationException vex)
        {
            return false;
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1", title = "Validation failed", status = 400, errors = vex.Errors
        }, ct);

        return true;
    }
}
