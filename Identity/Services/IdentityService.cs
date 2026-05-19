using Identity.Entities;
using Identity.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using ApplicationService.Services.Contracts;  
using ApplicationService.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtService _jwtService;

    public IdentityService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }
    
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
    public async Task<AuthResult> Register(string username, string password, string email)
    {
        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser != null)
            return AuthResult.Fail("Username already exists");

        existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
            return AuthResult.Fail("Email already exists");

        var user = new AppUser
        {
            UserName = username,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, password);

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

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the tokens on success,
    /// or failure information (e.g., invalid credentials) on error.
    /// </returns>
    public async Task<AuthResult> Login(string username, string password) 
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            user = await _userManager.FindByEmailAsync(username);

        if (user == null)
            return AuthResult.Fail("Invalid username or password");

        if (!user.IsActive)
            return AuthResult.Fail("Account is deactivated");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

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

    /// <summary>
    /// Generates a new access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token obtained during login or previous refresh.</param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing a new access token (and possibly a new refresh token)
    /// if the refresh token is valid; otherwise, failure information.
    /// </returns>
    public async Task<AuthResult> RefreshToken(string refreshToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

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

    /// <summary>
    /// Logs out the user by invalidating the refresh token or session.
    /// </summary>
    /// <param name="userId">The identifier of the user to log out.</param>
    /// <returns>
    /// <c>true</c> if logout was successful; otherwise <c>false</c> (e.g., user not found or already logged out).
    /// </returns>
    public async Task<bool> Logout(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return false;

        return true;
    }
}
