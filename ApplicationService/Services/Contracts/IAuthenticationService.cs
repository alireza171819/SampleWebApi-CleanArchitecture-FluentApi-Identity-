using ApplicationService.Common.Models;
using ApplicationService.Dtos.Authentications;
using ApplicationService.Dtos.Users;
using Domain.Common;

namespace ApplicationService.Services.Contracts;

public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user in both Identity and Domain databases.
    /// </summary>
    /// <param name="userRegisterDto">The data required to register a new user.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the registration completed successfully,
    /// or containing validation and registration errors.
    /// </returns>
    Task<Result> RegisterAsync(UserRegisterDto userRegisterDto, CancellationToken cancellationToken);


    /// <summary>
    /// Authenticates the user and returns authentication tokens. Also synchronizes
    /// the user's domain state if required.
    /// </summary>
    /// <param name="userLogInDto">The user credentials required for authentication.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing an <see cref="AuthResult"/> on success,
    /// or authentication error details if the operation fails.
    /// </returns>
    Task<Result<AuthResult>> LoginAsync(UserLoginDto userLogInDto, CancellationToken cancellationToken);


    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">The refresh token request data.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a new <see cref="AuthResult"/> if the refresh
    /// operation succeeds; otherwise, an error result.
    /// </returns>
    Task<Result<AuthResult>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken);


    /// <summary>
    /// Generates a password reset token for the user with the specified email address.
    /// </summary>
    /// <param name="forgotPasswordDto">
    /// The data required to generate a password reset token.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the password reset request
    /// was processed successfully.
    /// </returns>
    Task<Result> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto, CancellationToken cancellationToken);


    /// <summary>
    /// Changes the user's password after verifying the current password.
    /// </summary>
    /// <param name="changePasswordDto">
    /// The data required to change the user's password.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the outcome of the password change operation.
    /// </returns>
    Task<AuthResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto, CancellationToken cancellationToken);


    /// <summary>
    /// Confirms the user's email address using the confirmation token.
    /// </summary>
    /// <param name="confirmEmailDto">
    /// The data required to confirm the user's email address.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the email was confirmed successfully.
    /// </returns>
    Task<Result> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto, CancellationToken cancellationToken);


    /// <summary>
    /// Logs out the specified user by invalidating the stored refresh token.
    /// </summary>
    /// <param name="userByIdDto">The identifier of the user to log out.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the logout operation completed successfully.
    /// </returns>
    Task<Result> LogoutAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken);


    /// <summary>
    /// Retrieves the current user's information from both Identity and Domain stores.
    /// </summary>
    /// <param name="userByIdDto">The identifier of the user.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the user's information as a
    /// <see cref="UserSingleDto"/> if found; otherwise, an error result.
    /// </returns>
    Task<Result<UserSingleDto>> GetCurrentUserAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken);
}
