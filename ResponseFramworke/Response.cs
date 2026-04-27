using System.Net;

namespace ResponseFramworke;

public class Response<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public string? Message { get; }
    public HttpStatusCode HttpStatusCode { get; }

    protected Response(bool isSuccess, string? error = null, HttpStatusCode statusCode = default)
    {
        IsSuccess = isSuccess;
        ErrorMessage = error;
        HttpStatusCode = statusCode;
    }
    protected Response(T? value, bool isSuccess, string? errorMessage, HttpStatusCode statusCode = default)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Value = value;
        HttpStatusCode = statusCode;
    }

    public static Response<T> Success(T value) => new(value, true, null, HttpStatusCode.OK);

    public static Response<T> Failure(string errorMessage, HttpStatusCode statusCode) => new(false, errorMessage, statusCode);

    public static Response<T> NotFound(string errorMessage) => new(false, errorMessage, HttpStatusCode.NotFound);

    public static Response<T> BadRequest(string errorMessage) => new(false, errorMessage, HttpStatusCode.BadRequest);
}
