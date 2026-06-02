
using System.Net;

namespace Domain.Common;

/// <summary>
/// Represents the result of an operation, containing the value, success status, error message, and HTTP status code.
/// Based on the Result pattern from Ardalis.Result: https://github.com/ardalis/Result/blob/main/src/Ardalis.Result/Result.Void.cs
/// </summary>
public class Result : Result<Result>
{
    public Result() : base() { }

    /// <summary>
    /// Initializes a new result without a value.
    /// Typically used for failed results.
    /// </summary>
    /// <param name="isSuccess">
    /// Indicates whether the operation completed successfully.
    /// </param>
    /// <param name="errorMessage">
    /// The error message describing the failure, if any.
    /// </param>
    /// <param name="status">
    /// The status associated with the operation result.
    /// </param>
    protected Result(bool isSuccess, string? errorMessage, ResultStatus status) : base (default, isSuccess, errorMessage, status)
    {
    }

    /// <summary>
    /// Represents a successful operation without return type
    /// </summary>
    /// <returns>A Result</returns>
    public static Result Success() => new(true, string.Empty, ResultStatus.Ok);

    /// <summary>
    /// Creates a failed result with the specified error message and status.
    /// </summary>
    /// <param name="errorMessage">
    /// A description of the failure.
    /// </param>
    /// <param name="status">
    /// The status representing the failure reason.
    /// </param>
    /// <returns>
    /// A failed result.
    /// </returns>
    public static Result Failure(string errorMessage, ResultStatus status) => new(false, errorMessage, status);

    /// <summary>
    /// Creates a failed result with a NotFound status.
    /// </summary>
    /// <param name="errorMessage">
    /// Details about the missing resource.
    /// </param>
    /// <returns>
    /// A failed result with <see cref="ResultStatus.NotFound"/>.
    /// </returns>
    public static Result NotFound(string errorMessage) => new(false, errorMessage, ResultStatus.NotFound);

    /// <summary>
    /// Creates a failed result with a BadRequest status.
    /// </summary>
    /// <param name="errorMessage">
    /// Details about the invalid request.
    /// </param>
    /// <returns>
    /// A failed result with <see cref="ResultStatus.BadRequest"/>.
    /// </returns>
    public new static Result BadRequest(string errorMessage) => new(false, errorMessage, ResultStatus.BadRequest);
}
