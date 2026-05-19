using System.Security.Claims;

namespace Identity.Services.Interfaces;

/// <summary>
/// Provides JWT (JSON Web Token) generation and validation services.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a new access token for the specified user.
    /// </summary>
    /// <param name="id">The unique identifier of the user (typically from the user domain).</param>
    /// <param name="username">The username of the authenticated user.</param>
    /// <returns>A signed JWT access token as a string.</returns>
    string GenerateToken(Guid id, string username);

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// </summary>
    /// <returns>A base64-encoded refresh token string.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Retrieves the <see cref="ClaimsPrincipal"/> from an expired access token without validation of its expiration.
    /// </summary>
    /// <param name="token">The expired JWT access token.</param>
    /// <returns>
    /// A <see cref="ClaimsPrincipal"/> containing the claims from the token if the signature is valid;
    /// otherwise, <c>null</c>.
    /// </returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
