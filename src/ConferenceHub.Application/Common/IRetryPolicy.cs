namespace ConferenceHub.Application.Common;

public interface IRetryPolicy
{
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct);
}
