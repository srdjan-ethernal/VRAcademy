namespace VRAcademy.Api.Models;

public sealed class Result<T>
{
    private Result(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public string? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(string error) => new(default, error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess && Value is not null ? onSuccess(Value) : onFailure(Error ?? "Nepoznata greska.");
    }
}

