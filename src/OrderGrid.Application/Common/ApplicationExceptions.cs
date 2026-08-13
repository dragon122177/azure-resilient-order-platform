namespace OrderGrid.Application.Common;

public sealed class RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more request fields are invalid.")
{ public IReadOnlyDictionary<string, string[]> Errors { get; } = errors; }

public sealed class ResourceNotFoundException(string message) : Exception(message);
public sealed class ConflictException(string message) : Exception(message);
