using ApplicationService.Common;
using ApplicationService.Dtos.Authentications;
using ApplicationService.Dtos.Users;

namespace ApplicationService.Services.Contracts;

public interface IIdentityService
{
    /// <summary>
    /// Registers a new user with the specified credentials.
    /// </summary>
    /// <param name="userRegisterDto">Data transfer object containing required fields for registeration an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the authentication tokens (access and refresh)
    /// and any validation errors or failure information.
    /// </returns>
    Task<Result<AuthResult>> RegisterAsync(UserRegisterDto userRegisterDto);

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    ///  <param name="userLogInDto">Data transfer object containing required fields for login an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the tokens on success,
    /// or failure information (e.g., invalid credentials) on error.
    /// </returns>
    Task<Result<AuthResult>> LoginAsync(UserLoginDto userLogInDto);

    /// <summary>
    /// Generates a new access token using a valid refresh token.
    /// </summary>
    ///  <param name="refreshTokenDto">Data transfer object containing required fields for generate an token.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing a new access token (and possibly a new refresh token)
    /// if the refresh token is valid; otherwise, failure information.
    /// </returns>
    Task<Result<AuthResult>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

    /// <summary>
    /// Generates a password reset token for the user with the specified email.
    /// </summary>
    ///  <param name="forgotPasswordDto">Data transfer object containing required fields for generate a password reset token.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the reset token in the Token property if successful,
    /// or failure information if the email does not exist or user is inactive.
    /// </returns>
    Task<Result<AuthResult>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);

    /// <summary>
    /// Confirms the user's email address using the token generated during registration.
    /// </summary>
    /// <param name="confirmEmailDto">Data transfer object containing required fields for confirm the user's  email address.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure (e.g., invalid token, user not found).
    /// </returns>
    Task<Result<AuthResult>> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto);

    /// <summary>
    /// Changes the user's password after verifying the current password.
    /// </summary>
    ///  <param name="changePasswordDto">Data transfer object containing required fields for change user password.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure (e.g., wrong current password, weak new password).
    /// </returns>
    Task<Result<AuthResult>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);

    /// <summary>
    /// Logs out the user by invalidating the refresh token or session.
    /// </summary>
    ///  <param name="userByIdDto">Data transfer object containing required fields for logout an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure .
    /// </returns>
    Task<Result<AuthResult>> LogoutAsync(UserByIdDto userByIdDto);

    /// <summary>
    /// Permanently deletes a user.
    /// </summary>
    ///  <param name="userByIdDto">Data transfer object containing required fields for delete an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure .
    /// </returns>
    Task<Result<AuthResult>> DeleteUserAsync(UserByIdDto userByIdDto);

    /// <summary>
    /// Soft deletes a user (marks as deleted without removing the record).
    /// </summary>
    ///  <param name="userByIdDto">Data transfer object containing required fields for soft delete an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure .
    /// </returns>
    Task<Result<AuthResult>> SoftDeleteUserAsync(UserByIdDto userByIdDto);
}
