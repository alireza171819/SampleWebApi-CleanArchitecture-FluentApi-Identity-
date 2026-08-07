using System.Text.Json;
using ApplicationService.Common;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace SampleWebApi.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                "Unhandled exception occurred. TraceId:{TraceId}",
                context.TraceIdentifier);

            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var result = exception switch
        {
            DomainException ex =>
                Result.Invalid(ex.Message),

            ValidationException ex =>
                Result.Invalid(string.Join(" | ",
                    ex.Errors.Select(x => x.ErrorMessage))),

            UnauthorizedAccessException ex =>
                Result.Failure(ex.Message, ResultStatus.Unauthorized),

            KeyNotFoundException ex =>
                Result.Failure(ex.Message, ResultStatus.NotFound),

            OperationCanceledException =>
                Result.Failure(
                    "Request was cancelled.",
                    ResultStatus.ClientClosedRequest),

            _ =>
                Result.Failure(
                    "An unexpected error has occurred.",
                    ResultStatus.InternalServerError)
        };

        context.Response.StatusCode = GetStatusCode(result.Status);
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(result));
    }

    private static int GetStatusCode(ResultStatus status)
    {
        return status switch
        {
            ResultStatus.Ok => StatusCodes.Status200OK,

            ResultStatus.NoContent => StatusCodes.Status204NoContent,

            ResultStatus.BadRequest => StatusCodes.Status400BadRequest,

            ResultStatus.Invalid => StatusCodes.Status400BadRequest,

            ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,

            ResultStatus.Forbidden => StatusCodes.Status403Forbidden,

            ResultStatus.NotFound => StatusCodes.Status404NotFound,

            ResultStatus.Conflict => StatusCodes.Status409Conflict,

            ResultStatus.Unavailable => StatusCodes.Status503ServiceUnavailable,

            ResultStatus.CriticalError => StatusCodes.Status500InternalServerError,

            ResultStatus.InternalServerError => StatusCodes.Status500InternalServerError,

            ResultStatus.ClientClosedRequest => 499,

            _ => StatusCodes.Status500InternalServerError
        };
    }
}