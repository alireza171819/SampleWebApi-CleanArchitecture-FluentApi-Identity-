using System.Net;

namespace Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public string? Message { get; }
    public HttpStatusCode HttpStatusCode { get; }

    protected Result(bool isSuccess, string? error = null, HttpStatusCode statusCode = default)
    {
        IsSuccess = isSuccess;
        ErrorMessage = error;
        HttpStatusCode = statusCode;
    }
    protected Result(T? value, bool isSuccess, string? errorMessage, HttpStatusCode statusCode = default)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Value = value;
        HttpStatusCode = statusCode;
    }

    public static Result<T> Success(T value) => new(value, true, null, HttpStatusCode.OK);

    public static Result<T> Failure(string errorMessage, HttpStatusCode statusCode) => new(false, errorMessage, statusCode);

    public static Result<T> NotFound(string errorMessage) => new(false, errorMessage, HttpStatusCode.NotFound);

    public static Result<T> BadRequest(string errorMessage) => new(false, errorMessage, HttpStatusCode.BadRequest);
}
