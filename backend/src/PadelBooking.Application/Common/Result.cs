namespace PadelBooking.Application.Common;

/// <summary>
/// Wraps a service operation's outcome so controllers can translate failures into
/// proper HTTP responses (400/404/409) without services throwing exceptions for
/// expected, user-facing failure cases.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, T? value, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, null, ResultErrorType.None);

    public static Result<T> Failure(string error, ResultErrorType errorType = ResultErrorType.Validation) =>
        new(false, default, error, errorType);
}

public enum ResultErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Unauthorized
}
