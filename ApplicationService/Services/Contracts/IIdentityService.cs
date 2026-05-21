using ApplicationService.Common.Models;

namespace ApplicationService.Services.Contracts;

public interface IIdentityService
{
    /// <summary>
    /// Registers a new user with the specified credentials.
    /// </summary>
    /// <param name="username">The unique username for the new user.</param>
    /// <param name="password">The password for the new user.</param>
    /// <param name="email">The email address of the new user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the authentication tokens (access and refresh)
    /// and any validation errors or failure information.
    /// </returns>
    Task<AuthResult> Register(string username, string password, string email);

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the tokens on success,
    /// or failure information (e.g., invalid credentials) on error.
    /// </returns>
    Task<AuthResult> Login(string username, string password);

    /// <summary>
    /// Generates a new access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token obtained during login or previous refresh.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing a new access token (and possibly a new refresh token)
    /// if the refresh token is valid; otherwise, failure information.
    /// </returns>
    Task<AuthResult> RefreshToken(string refreshToken);

    /// <summary>
    /// Logs out the user by invalidating the refresh token or session.
    /// </summary>
    /// <param name="userId">The identifier of the user to log out.</param>
    /// <returns>
    /// <c>true</c> if logout was successful; otherwise <c>false</c> (e.g., user not found or already logged out).
    /// </returns>
    Task<bool> Logout(Guid userId);

    /// <summary>
    /// Permanently deletes a user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>True if deleted successfully, otherwise false.</returns>
    Task<bool> DeleteUser(Guid userId);

    /// <summary>
    /// Soft deletes a user (marks as deleted without removing the record).
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>True if soft deleted successfully, otherwise false.</returns>
    Task<bool> SoftDeleteUser(Guid userId);
}
