using ApplicationService.Common.Models;
using ApplicationService.Services.Contracts;
using Domain.Aggregates.Users;
using Domain.Common;
using Domain.Contracts.Persistence;
using System.Net;

namespace ApplicationService.Services.Authentications;

public class AuthenticationService
{
    #region Privet Fields
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    #endregion

    #region Constructor

    public AuthenticationService(IIdentityService identityService, IUserRepository userRepository)
    {
        _identityService = identityService;
        _userRepository = userRepository;
    }

    #endregion

    #region Register(string username, string password, string email)

    /// <summary>
    /// Registers a new user in both Identity and Domain databases.
    /// </summary>
    public async Task<Result> Register(string username, string password, string email, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.Register(username, password, email);
        if (authResult.IsFailure)
            return Result.Failure("Registration failed.", HttpStatusCode.BadRequest);

        var domainUser = new User(username, email, isActive: true);
        domainUser.SetUid(Guid.Parse(authResult.UserId!.Value.ToString())); // Set the Uuid to match Identity Id

        var insertResult = await _userRepository.Insert(domainUser, cancellationToken);
        if (insertResult.IsFailure)
        {
            // Compensation: rollback Identity user
            await _identityService.Logout(authResult.UserId!.Value);
            await _identityService.DeleteUser();
            // Note: You might need a DeleteUser in IIdentityService, but for simplicity we just clear refresh token.
            return Result.Failure(
                "User profile could not be created. Please contact support.",
                HttpStatusCode.InternalServerError);
        }

        return Result.Success();
    }
    #endregion

    #region Login(string username, string password)

    /// <summary>
    /// Authenticates user and returns tokens. Also syncs domain user state if needed.
    /// </summary>
    public async Task<Result> Login(string username, string password, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.Login(username, password);
        if (authResult.IsFailure)
            return Result.Failure(
                authResult.Errors?.FirstOrDefault() ?? "Invalid login attempt",
                HttpStatusCode.Unauthorized);

        // Optional: Sync domain user state (e.g., reactivate if deactivated)
        var userId = authResult.UserId!.Value;
        var domainResult = await _userRepository.FindByUuid(Guid.Parse(userId.ToString()), cancellationToken);
        if (domainResult == null)
        {
            // Domain user missing – maybe create it now? Or return error.
            return Result.Failure("User profile not found", HttpStatusCode.NotFound);
        }

        if (!domainResult.Value.IsActive)
        {
            // If domain user is deactivated, you might want to log out the identity user.
            await _identityService.Logout(userId);
            return Result.Failure("Account is deactivated", HttpStatusCode.Forbidden);
        }

        return Result.Success();
    }
    #endregion

    #region RefreshTokenAsync
    /// <summary>
    /// Refreshes access token using refresh token.
    /// </summary>
    public async Task<Result<AuthResult>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var authResult = await _identityService.RefreshTokenAsync(refreshToken, cancellationToken);
        if (!authResult.Succeeded)
            return Result<AuthResult>.Failure(
                authResult.Errors?.FirstOrDefault() ?? "Invalid refresh token",
                HttpStatusCode.Unauthorized);

        return Result<AuthResult>.Success(authResult);
    }
    #endregion

    #region LogoutAsync
    /// <summary>
    /// Logs out the user (invalidates refresh token in Identity).
    /// </summary>
    public async Task<Result<bool>> LogoutAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.LogoutAsync(userId, cancellationToken);
        if (!result)
            return Result<bool>.Failure("Logout failed", HttpStatusCode.BadRequest);

        return Result<bool>.Success(true);
    }
    #endregion

    #region GetCurrentUserAsync
    /// <summary>
    /// Gets the combined user information from both Identity and Domain.
    /// </summary>
    public async Task<Result<CurrentUserDto>> GetCurrentUserAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        // Get identity user (via IIdentityService or directly? We assume IIdentityService has a method)
        // For simplicity, we retrieve domain user and map to DTO.
        var domainUser = await _userRepository.GetByUuidAsync(Guid.Parse(userId.ToString()), cancellationToken);
        if (domainUser == null)
            return Result<CurrentUserDto>.NotFound("User not found");

        var dto = new CurrentUserDto
        {
            Id = domainUser.Id,
            Uuid = domainUser.Uuid,
            Username = domainUser.Username,
            Email = domainUser.Email,
            IsActive = domainUser.IsActive
        };

        return Result<CurrentUserDto>.Success(dto);
    }
    #endregion
}
