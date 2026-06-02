using ApplicationService.Services.Contracts;
using Identity.Services;
using Identity.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityDependency(
    this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtService, JwtService>();
        return services;
    }
}
