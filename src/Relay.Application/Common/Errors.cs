namespace Relay.Application.Common;

public sealed class ValidationFailureException(IReadOnlyList<string> errors) : Exception("Request validation failed.")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}

public sealed class NotFoundException(string resource) : Exception($"{resource} was not found.");
public sealed class ForbiddenOperationException(string message) : Exception(message);
public sealed class ConflictException(string message) : Exception(message);
