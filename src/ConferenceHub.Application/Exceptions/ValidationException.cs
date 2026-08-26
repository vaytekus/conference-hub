namespace ConferenceHub.Application.Exceptions;

public class ValidationException(IEnumerable<string> errors) : Exception("Validation failed")
{
    public IReadOnlyList<string> Errors { get; } = errors.ToList();
}
