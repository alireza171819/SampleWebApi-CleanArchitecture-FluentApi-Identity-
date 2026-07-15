using ApplicationService.Common.Models;
using ApplicationService.Dtos.Authentications;
using ApplicationService.Dtos.Users;
using ApplicationService.Services.Contracts;  
using Identity.Entities;
using Identity.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services;

public class IdentityService : IIdentityService
{
    #region Privet Fields
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtService _jwtService;
    #endregion

    #region Constructor
    public IdentityService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }
    #endregion

    #region Register(UserRegisterDto userRegisterDto)
    /// <summary>
    /// Registers a new user with the specified credentials.
    /// </summary>
    /// <param name="userRegisterDto">Data transfer object containing required fields for registeration an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the authentication tokens (access and refresh)
    /// and any validation errors or failure information.
    /// </returns>
    public async Task<AuthResult> RegisterAsync(UserRegisterDto userRegisterDto)
    {
        var existingUser = await _userManager.FindByNameAsync(userRegisterDto.Username);
        if (existingUser != null)
            return AuthResult.Fail("Username already exists");

        existingUser = await _userManager.FindByEmailAsync(userRegisterDto.Email);
        if (existingUser != null)
            return AuthResult.Fail("Email already exists");

        var user = new AppUser
        {
            UserName = userRegisterDto.Username,
            Email = userRegisterDto.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, userRegisterDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return AuthResult.Fail(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
            return AuthResult.Fail("Failed to save refresh token");

        var token = _jwtService.GenerateToken(user.Id, user.UserName!);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return AuthResult.Fail("Failed to save refresh token");

        return AuthResult.Ok(token, refreshToken, user.Id, user.UserName!);
    }
    #endregion

    #region Login(UserLogInDto userLogInDto)
    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    ///  <param name="userLogInDto">Data transfer object containing required fields for login an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the tokens on success,
    /// or failure information (e.g., invalid credentials) on error.
    /// </returns>
    public async Task<AuthResult> LoginAsync(UserLoginDto userLogInDto)
    {
        var user = await _userManager.FindByNameAsync(userLogInDto.Username);
        if (user == null)
            user = await _userManager.FindByEmailAsync(userLogInDto.Username);

        if (user == null)
            return AuthResult.Fail("Invalid username or password");

        if (!user.IsActive)
            return AuthResult.Fail("Account is deactivated");

        var result = await _signInManager.CheckPasswordSignInAsync(user, userLogInDto.Password, false);

        if (!result.Succeeded)
            return AuthResult.Fail("Invalid username or password");

        var token = _jwtService.GenerateToken(user.Id, user.UserName!);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return AuthResult.Fail("Failed to save refresh token");

        return AuthResult.Ok(token, refreshToken, user.Id, user.UserName!);
    }
    #endregion

    #region RefreshToken(RefreshTokenDto refreshTokenDto)
    /// <summary>
    /// Generates a new access token using a valid refresh token.
    /// </summary>
    ///  <param name="refreshTokenDto">Data transfer object containing required fields for generate an token.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing a new access token (and possibly a new refresh token)
    /// if the refresh token is valid; otherwise, failure information.
    /// </returns>
    public async Task<AuthResult> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenDto.RefreshToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return AuthResult.Fail("Invalid refresh token");

        var newToken = _jwtService.GenerateToken(user.Id, user.UserName!);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return AuthResult.Fail("Failed to save refresh token");

        return AuthResult.Ok(newToken, newRefreshToken, user.Id, user.UserName!);
    }
    #endregion

    #region ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    /// <summary>
    /// Generates a password reset token for the user with the specified email.
    /// </summary>
    ///  <param name="forgotPasswordDto">Data transfer object containing required fields for generate a password reset token.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the reset token in the Token property if successful,
    /// or failure information if the email does not exist or user is inactive.
    /// </returns>
    public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        if (string.IsNullOrWhiteSpace(forgotPasswordDto.Email))
            return AuthResult.Fail("Email is required");

        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (user == null || !user.IsActive)
            return AuthResult.Fail("If the email exists and the account is active, a reset link will be sent.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // In a real application, you would send this token via email.
        // Here we return the token to the caller (e.g., a service that will send the email).
        return AuthResult.Ok(token, refreshToken: null, user.Id, user.UserName!);
    }
    #endregion

    #region ConfirmEmail(ConfirmEmailDto confirmEmailDto)
    /// <summary>
    /// Confirms the user's email address using the token generated during registration.
    /// </summary>
    /// <param name="confirmEmailDto">Data transfer object containing required fields for confirm the user's  email address.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure (e.g., invalid token, user not found).
    /// </returns>
    public async Task<AuthResult> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
    {
        if (confirmEmailDto.UserUuid == Guid.Empty || string.IsNullOrWhiteSpace(confirmEmailDto.Token))
            return AuthResult.Fail("Invalid request");

        var user = await _userManager.FindByIdAsync(confirmEmailDto.UserUuid.ToString());
        if (user == null)
            return AuthResult.Fail("User not found");

        var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDto.Token);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return AuthResult.Fail(errors);
        }

        return AuthResult.Ok(token: null, refreshToken: null, user.Id, user.UserName!);
    }
    #endregion

    #region ChangePassword(ChangePasswordDto changePasswordDto)
    /// <summary>
    /// Changes the user's password after verifying the current password.
    /// </summary>
    ///  <param name="changePasswordDto">Data transfer object containing required fields for change user password.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure (e.g., wrong current password, weak new password).
    /// </returns>
    public async Task<AuthResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        if (changePasswordDto.UserUuid == Guid.Empty)
            return AuthResult.Fail("Invalid user identifier");

        var user = await _userManager.FindByIdAsync(changePasswordDto.UserUuid.ToString());
        if (user == null)
            return AuthResult.Fail("User not found");

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return AuthResult.Fail(errors);
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return AuthResult.Ok(token: null, refreshToken: null, user.Id, user.UserName!);
    }
    #endregion

    #region Logout(UserByIdDto userByIdDto)
    /// <summary>
    /// Logs out the user by invalidating the refresh token or session.
    /// </summary>
    ///  <param name="userByIdDto">Data transfer object containing required fields for logout an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure .
    /// </returns>
    public async Task<AuthResult> LogoutAsync(UserByIdDto userByIdDto)
    {
        if (userByIdDto.Uuid == Guid.Empty)
            return AuthResult.Fail("Invalid user identifier");

        var user = await _userManager.FindByIdAsync(userByIdDto.Uuid.ToString());
        if (user == null)
            return AuthResult.Fail("User not found");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return AuthResult.Fail("Logout faild.");

        return AuthResult.Ok(token: null, refreshToken: null, user.Id, user.UserName!);
    }
    #endregion

    #region DeleteUser(UserByIdDto userByIdDto)
    /// <summary>
    /// Permanently deletes a user.
    /// </summary>
    ///  <param name="userByIdDto">Data transfer object containing required fields for delete an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure .
    /// </returns>
    public async Task<AuthResult> DeleteUserAsync(UserByIdDto userByIdDto)
    {
        if (userByIdDto.Uuid == Guid.Empty)
            return AuthResult.Fail("Invalid user identifier");

        var user = await _userManager.FindByIdAsync(userByIdDto.Uuid.ToString());

        if (user == null)
            return AuthResult.Fail("User not found");

        var deleteResult = await _userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
            return AuthResult.Fail("Delete user faild.");

        return AuthResult.Ok(token: null, refreshToken: null, user.Id, user.UserName!);
    }
    #endregion

    #region SoftDeleteUser(UserByIdDto userByIdDto)
    /// <summary>
    /// Soft deletes a user (marks as deleted without removing the record).
    /// </summary>
    ///  <param name="userByIdDto">Data transfer object containing required fields for soft delete an user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> indicating success or failure .
    /// </returns>
    public async Task<AuthResult> SoftDeleteUserAsync(UserByIdDto userByIdDto)
    {
        if (userByIdDto.Uuid == Guid.Empty)
            return AuthResult.Fail("Invalid user identifier");

        var user = await _userManager.FindByIdAsync(userByIdDto.Uuid.ToString());

        if (user == null)
            return AuthResult.Fail("User not found");

        user.IsDelete = true;
        var deleteResult = await _userManager.UpdateAsync(user);

        if (!deleteResult.Succeeded)
            return AuthResult.Fail("Delete user faild.");

        return AuthResult.Ok(token: null, refreshToken: null, user.Id, user.UserName!);
    }
    #endregion
}
