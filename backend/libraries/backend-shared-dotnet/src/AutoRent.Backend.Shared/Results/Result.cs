namespace AutoRent.Backend.Shared.Results;

public sealed record ResultError(string Code, string Message, int? StatusCode = null);

public class Result
{
    protected Result(bool succeeded, ResultError? error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public bool Succeeded { get; }

    public bool Failed => !Succeeded;

    public ResultError? Error { get; }

    public static Result Success()
    {
        return new Result(true, null);
    }

    public static Result Failure(string code, string message, int? statusCode = null)
    {
        return new Result(false, new ResultError(code, message, statusCode));
    }

    public static Result Failure(ResultError error)
    {
        return new Result(false, error);
    }
}

public sealed class Result<T> : Result
{
    private Result(bool succeeded, T? value, ResultError? error)
        : base(succeeded, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    public new static Result<T> Failure(string code, string message, int? statusCode = null)
    {
        return new Result<T>(false, default, new ResultError(code, message, statusCode));
    }

    public new static Result<T> Failure(ResultError error)
    {
        return new Result<T>(false, default, error);
    }
}
