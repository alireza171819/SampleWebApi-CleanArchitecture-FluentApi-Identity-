using Identity.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Services;

public class JwtService : IJwtService
{
    #region Filds

    private readonly IConfiguration _configuration;

    #endregion

    #region Constructor
    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    #endregion

    #region GenerateToken(Guid userId, string username)

    /// <summary>
    /// Generates a new access token for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user (typically from the user domain).</param>
    /// <param name="username">The username of the authenticated user.</param>
    /// <returns>A signed JWT access token as a string.</returns>
    public string GenerateToken(Guid userId, string username)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    #endregion

    #region GenerateRefreshToken()

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// </summary>
    /// <returns>A base64-encoded refresh token string.</returns>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    #endregion

    #region GetPrincipalFromExpiredToken(string token)
    /// <summary>
    /// Retrieves the <see cref="ClaimsPrincipal"/> from an expired access token without validation of its expiration.
    /// </summary>
    /// <param name="token">The expired JWT access token.</param>
    /// <returns>
    /// A <see cref="ClaimsPrincipal"/> containing the claims from the token if the signature is valid;
    /// otherwise, <c>null</c>.
    /// </returns>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false  // Don't check expiration
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }
    #endregion
}
