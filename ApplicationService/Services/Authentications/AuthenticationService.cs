using ApplicationService.Common.Models;
using ApplicationService.Dtos.Authentications;
using ApplicationService.Dtos.Users;
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

    #region Register(UserRegisterDto userRegisterDto)

    /// <summary>
    /// Registers a new user in both Identity and Domain databases.
    /// </summary>
    public async Task<Result> Register(UserRegisterDto userRegisterDto, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.Register(userRegisterDto.Username, userRegisterDto.Password, userRegisterDto.Email);
        if (authResult.IsFailure)
            return Result.Failure("Registration failed.", HttpStatusCode.BadRequest);

        var domainUser = new User(userRegisterDto.Username, userRegisterDto.Email, isActive: true);
        domainUser.SetUid(authResult.UserId!.Value); // Set the Uuid to match Identity Id

        var insertResult = await _userRepository.Insert(domainUser, cancellationToken);
        if (insertResult.IsFailure)
        {
            // Compensation: rollback Identity user
            await _identityService.Logout(authResult.UserId!.Value);
            await _identityService.DeleteUser(authResult.UserId!.Value);
            // Note: You might need a DeleteUser in IIdentityService, but for simplicity we just clear refresh token.
            return Result.Failure(
                "User profile could not be created. Please contact support.",
                HttpStatusCode.InternalServerError);
        }

        return Result.Success();
    }
    #endregion

    #region Login(UserLogInDto userLogInDto)

    /// <summary>
    /// Authenticates user and returns tokens. Also syncs domain user state if needed.
    /// </summary>
    public async Task<Result<AuthResult>> Login(UserLogInDto userLogInDto, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.Login(userLogInDto.Username, userLogInDto.Password);
        if (authResult.IsFailure)
            return Result<AuthResult>.Failure(
                authResult.Errors?.FirstOrDefault() ?? "Invalid login attempt",
                HttpStatusCode.Unauthorized);

        // Optional: Sync domain user state (e.g., reactivate if deactivated)
        var userId = authResult.UserId!.Value;
        var domainResult = await _userRepository.FindByUuid(userId, cancellationToken);
        if (domainResult == null)
        {
            await _identityService.Logout(userId);
            return Result<AuthResult>.Failure("User profile not found", HttpStatusCode.NotFound);
        }

        if (!domainResult.Value.IsDeleted)
        {
            // If domain user is deactivated, you might want to log out the identity user.
            await _identityService.Logout(userId);
            return Result<AuthResult>.Failure("Account is deactivated", HttpStatusCode.Forbidden);
        }

        return Result<AuthResult>.Success(authResult);
    }
    #endregion

    #region RefreshTokenAsync
    /// <summary>
    /// Refreshes access token using refresh token.
    /// </summary>
    public async Task<Result<AuthResult>> RefreshToken(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var authResult = await _identityService.RefreshToken(refreshToken);
        if (authResult.IsFailure)
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
    public async Task<Result> LogoutAsync( Guid userId, CancellationToken cancellationToken)
    {
        var result = await _identityService.Logout(userId);
        if (!result)
            return Result.Failure("Logout failed", HttpStatusCode.BadRequest);

        return Result.Success();
    }
    #endregion

    #region GetCurrentUserAsync
    /// <summary>
    /// Gets the combined user information from both Identity and Domain.
    /// </summary>
    public async Task<Result<UserSingleDto>> GetCurrentUserAsync( Guid userId, CancellationToken cancellationToken)
    {
        // Get identity user (via IIdentityService or directly? We assume IIdentityService has a method)
        // For simplicity, we retrieve domain user and map to DTO.
        var domainUser = await _userRepository.FindByUuid(userId, cancellationToken);
        if (domainUser.IsFailure)
            return Result<UserSingleDto>.NotFound("User not found");

        var userDto = new UserSingleDto
        {
            Id = domainUser.Value.Id,
            Uuid = domainUser.Value.Uuid,
            Username = domainUser.Value.Username,
            Email = domainUser.Value.Email
        };

        return Result<UserSingleDto>.Success(userDto);
    }
    #endregion
}
