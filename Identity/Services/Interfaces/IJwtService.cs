using System.Security.Claims;

namespace Identity.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(int userId, string username);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
