using System.Net;

namespace Domain.Common;

/// <summary>
/// Represents the result of an operation, containing the value, success status, error message, and HTTP status code.
/// Based on the Result pattern from Ardalis.Result: https://github.com/ardalis/Result/blob/main/src/Ardalis.Result/Result.cs
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
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

    /// <summary>
    /// Creates a successful Result with the provided value.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A successful result with HTTP 200 OK status.</returns>
    public static Result<T> Success(T value) => new(value, true, null, HttpStatusCode.OK);

    /// <summary>
    /// Creates a failed Result with the specified error message and HTTP status code.
    /// </summary>
    /// <param name="errorMessage">Description of the failure.</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure(string errorMessage, HttpStatusCode statusCode) => new(false, errorMessage, statusCode);

    /// <summary>
    /// Creates a "Not Found" failed result with HTTP 404 status.
    /// </summary>
    /// <param name="errorMessage">Details about what was not found.</param>
    /// <returns>A failed result with 404 NotFound status.</returns>
    public static Result<T> NotFound(string errorMessage) => new(false, errorMessage, HttpStatusCode.NotFound);

    /// <summary>
    /// Creates a "Bad Request" failed result with HTTP 400 status.
    /// </summary>
    /// <param name="errorMessage">Description of the invalid request.</param>
    /// <returns>A failed result with 400 BadRequest status.</returns>
    public static Result<T> BadRequest(string errorMessage) => new(false, errorMessage, HttpStatusCode.BadRequest);
}
