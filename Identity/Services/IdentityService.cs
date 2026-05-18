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

    public async Task<AuthResult> RegisterAsync(string username, string password, string email)
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

        await _userManager.AddToRoleAsync(user, "User");

        var token = _jwtService.GenerateToken(user.Id, user.UserName!);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return AuthResult.Ok(token, refreshToken, user.Id, user.UserName!);
    }

    public async Task<AuthResult> LoginAsync(string username, string password) 
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
        await _userManager.UpdateAsync(user);

        return AuthResult.Ok(token, refreshToken, user.Id, user.UserName!);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return AuthResult.Fail("Invalid refresh token");

        var newToken = _jwtService.GenerateToken(user.Id, user.UserName!);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return AuthResult.Ok(newToken, newRefreshToken, user.Id, user.UserName!);
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return true;
    }

    //public async Task<User?> GetUserByIdAsync(int userId)
    //{
    //    var appUser = await _userManager.FindByIdAsync(userId.ToString());
    //    if (appUser == null) return null;

    //    return new User(appUser.UserName!, appUser.Email!, appUser.IsActive)
    //    {
    //        Id = appUser.Id,
    //        RefreshToken = appUser.RefreshToken,
    //        RefreshTokenExpiry = appUser.RefreshTokenExpiry
    //    };
    //}
}
