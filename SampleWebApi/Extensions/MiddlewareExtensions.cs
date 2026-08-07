using SampleWebApi.Middlewares;

namespace SampleWebApi.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalException(this IApplicationBuilder app) => app.UseMiddleware<GlobalExceptionMiddleware>();
}
