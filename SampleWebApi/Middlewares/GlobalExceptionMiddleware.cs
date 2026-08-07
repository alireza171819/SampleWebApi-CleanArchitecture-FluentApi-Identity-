using SampleWebApi.Helpers;

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
        var result = ExceptionMapper.Map(exception);

        context.Response.StatusCode = StatusCodeMapper.Map(result.Status);
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(result);
    }
}