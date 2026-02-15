namespace GigaChess.Api.Common;

public class Result<T>
{
    public bool Success { get; init; }
    public bool IsNotFound { get; init; }
    public string? Error { get; init; }
    public T? Data { get; init; }

    public static Result<T> Ok(T data) => new() { Success = true, Data = data };
    public static Result<T> Fail(string error) => new() { Error = error };
    public static Result<T> NotFound(string error) => new() { Error = error, IsNotFound = true };
}
