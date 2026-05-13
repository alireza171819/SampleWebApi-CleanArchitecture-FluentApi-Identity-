using ApplicationService.Common.Models;

namespace ApplicationService.Services.Contracts;

public interface IIdentityService
{
    Task<AuthResult> RegisterAsync(string username, string password, string email);
    Task<AuthResult> LoginAsync(string username, string password);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(int userId);
}
