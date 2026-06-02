using ApplicationService.Services.Authentications;
using ApplicationService.Services.Contracts;
using ApplicationService.Services.Orders;
using ApplicationService.Services.Products;
using ApplicationService.Services.Users;
using ApplicationService.Validators.Authentications;
using ApplicationService.Validators.Orders;
using ApplicationService.Validators.Products;
using ApplicationService.Validators.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationService.Configurations;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
    this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<ProductUpdateDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<ChangePasswordDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<ConfirmEmailDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<ForgotPasswordDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<RefreshTokenDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UserLogInDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UserRegisterDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<OrderCreateDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<OrderDetailSingleDtoValidator>(); 
        services.AddValidatorsFromAssemblyContaining<OrderUpdateDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UserCreateDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UserUpdateDtoValidator>();
        return services;
    }
}
