using ConferenceHub.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConferenceHub.Application.Common;

public class SerializationRetryPolicy : IRetryPolicy
{
    private const int MaxAttempts = 3;
    private const int RetryBackoffMillisPerAttempt = 50;

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await operation(ct);
            }
            catch (Exception ex) when (IsSerializationFailure(ex))
            {
                if (attempt >= MaxAttempts)
                {
                    throw new ConflictException("Time slot is already booked for this room");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(RetryBackoffMillisPerAttempt * attempt), ct);
            }
        }
    }

    private static bool IsSerializationFailure(Exception ex) => ex switch
    {
        PostgresException pg
            => pg.SqlState == PostgresErrorCodes.SerializationFailure,
        DbUpdateConcurrencyException { InnerException: PostgresException pg }
            => pg.SqlState == PostgresErrorCodes.SerializationFailure,
        _ => false
    };
}
