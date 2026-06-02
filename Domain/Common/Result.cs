
namespace Domain.Common;

/// <summary>
/// Represents the result of an operation, containing the value, success status, error message, and HTTP status code.
/// Based on the Result pattern from Ardalis.Result: https://github.com/ardalis/Result/blob/main/src/Ardalis.Result/Result.cs
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T>
{
    protected Result() { }

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
    protected Result(T? value, bool isSuccess, string? errorMessage, ResultStatus status)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Value = value;
        Status = status;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public ResultStatus Status { get; }


    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <param name="value">
    /// The value returned by the operation.
    /// </param>
    /// <returns>
    /// A successful result with <see cref="ResultStatus.Ok"/>.
    /// </returns>
    public static Result<T> Success(T value) => new(value, true, string.Empty, ResultStatus.Ok);

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
    public static Result<T> Failure(string errorMessage, ResultStatus statusCode) => new(default, false, errorMessage, statusCode);

    /// <summary>
    /// Creates a failed result with a NotFound status.
    /// </summary>
    /// <param name="errorMessage">
    /// Details about the missing resource.
    /// </param>
    /// <returns>
    /// A failed result with <see cref="ResultStatus.NotFound"/>.
    /// </returns>
    public static Result<T> NotFound(string errorMessage) => new(default, false, errorMessage, ResultStatus.NotFound);

    /// <summary>
    /// Creates a failed result with a BadRequest status.
    /// </summary>
    /// <param name="errorMessage">
    /// Details about the invalid request.
    /// </param>
    /// <returns>
    /// A failed result with <see cref="ResultStatus.BadRequest"/>.
    /// </returns>
    public static Result<T> BadRequest(string errorMessage) => new(default, false, errorMessage, ResultStatus.BadRequest);
}
