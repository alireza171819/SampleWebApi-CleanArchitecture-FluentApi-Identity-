using ApplicationService.Common;
using Domain.Exceptions;
using FluentValidation;

namespace SampleWebApi.Helpers;

public static class ExceptionMapper
{
    public static Result Map(Exception exception)
    {
        return exception switch
        {
            DomainException ex =>
                Result.Failure(ex.Message, ResultStatus.BadRequest),

            ValidationException ex =>
                Result.Failure(
                    string.Join(Environment.NewLine,
                        ex.Errors.Select(x => x.ErrorMessage)),
                    ResultStatus.Invalid),

            UnauthorizedAccessException ex =>
                Result.Failure(ex.Message, ResultStatus.Unauthorized),

            KeyNotFoundException ex =>
                Result.Failure(ex.Message, ResultStatus.NotFound),

            OperationCanceledException =>
                Result.Failure(
                    "The request was cancelled.",
                    ResultStatus.ClientClosedRequest),

            NotImplementedException =>
                Result.Failure(
                    "Feature is not implemented.",
                    ResultStatus.Error),

            TimeoutException =>
                Result.Failure(
                    "The operation timed out.",
                    ResultStatus.Unavailable),

            _ =>
                Result.Failure(
                    "An unexpected error occurred.",
                    ResultStatus.InternalServerError)
        };
    }
}
