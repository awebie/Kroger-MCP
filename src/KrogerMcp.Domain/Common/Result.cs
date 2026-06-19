namespace KrogerMcp.Domain.Common;

public sealed record Result<T>
{
    private Result(T? value, KrogerError? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }
    public KrogerError? Error { get; }
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(KrogerError error) => new(default, error);
}

public sealed record KrogerError(ErrorCategory Category, string Message, int? StatusCode = null, string? Code = null)
{
    public static KrogerError Validation(string message) => new(ErrorCategory.InvalidInput, message);
}

public enum ErrorCategory
{
    InvalidInput,
    Unauthorized,
    Forbidden,
    NotFound,
    UpstreamUnavailable,
    UnexpectedUpstreamResponse,
    Configuration
}
