
using System.Net;

namespace Domain.Common;

/// <summary>
/// Represents the result of an operation, containing the value, success status, error message, and HTTP status code.
/// Based on the Result pattern from Ardalis.Result: https://github.com/ardalis/Result/blob/main/src/Ardalis.Result/Result.Void.cs
/// </summary>
public class Result : Result<Result>
{
    public Result() : base() { }

    protected Result(bool isSuccess, string? message = null, string? error = null, HttpStatusCode statusCode = default) : base (isSuccess, error, statusCode)
    {
    }

    /// <summary>
    /// Represents a successful operation without return type
    /// </summary>
    /// <returns>A Result</returns>
    public static Result Success() => new();

    /// <summary>
    /// Creates a failed Result with the specified error message and HTTP status code.
    /// </summary>
    /// <param name="errorMessage">Description of the failure.</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <returns>A failed result.</returns>
    public static Result Failure(string errorMessage, HttpStatusCode statusCode) => new(false, string.Empty, errorMessage, statusCode);

    /// <summary>
    /// Creates a "Not Found" failed result with HTTP 404 status.
    /// </summary>
    /// <param name="errorMessage">Details about what was not found.</param>
    /// <returns>A failed result with 404 NotFound status.</returns>
    public static Result NotFound(string errorMessage) => new(false, string.Empty, errorMessage, HttpStatusCode.NotFound);

    /// <summary>
    /// Creates a "Bad Request" failed result with HTTP 400 status.
    /// </summary>
    /// <param name="errorMessage">Description of the invalid request.</param>
    /// <returns>A failed result with 400 BadRequest status.</returns>
    public new static Result BadRequest(string errorMessage) => new(false, string.Empty, errorMessage, HttpStatusCode.BadRequest);
}
