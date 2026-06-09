using ApplicationService.Common.Models;
using ApplicationService.Dtos.Authentications;
using ApplicationService.Dtos.Users;
using ApplicationService.Services.Contracts;
using Domain.Aggregates.Users;
using Domain.Common;
using Domain.Contracts.Persistence;

namespace ApplicationService.Services.Authentications;

public class AuthenticationService : IAuthenticationService
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
        var authResult = await _identityService.Register(userRegisterDto);
        if (authResult.IsFailure)
            return Result.Failure("Registration failed.", ResultStatus.BadRequest);

        var domainUser = new User(userRegisterDto.Username, userRegisterDto.Email);
        domainUser.SetUid(authResult.UserId!.Value); // Set the Uuid to match Identity Id

        var insertResult = await _userRepository.Insert(domainUser, cancellationToken);
        if (insertResult.IsFailure)
        {
            // Compensation: rollback Identity user
            await _identityService.Logout(new UserByIdDto { Uuid = authResult.UserId.Value});
            await _identityService.DeleteUser(new UserByIdDto { Uuid = authResult.UserId.Value });
            // Note: You might need a DeleteUser in IIdentityService, but for simplicity we just clear refresh token.
            return Result.Failure(
                "User profile could not be created. Please contact support.",
                ResultStatus.InternalServerError);
        }

        return Result.Success();
    }
    #endregion

    #region Login(UserLogInDto userLogInDto)

    /// <summary>
    /// Authenticates user and returns tokens. Also syncs domain user state if needed.
    /// </summary>
    public async Task<Result<AuthResult>> Login(UserLoginDto userLogInDto, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.Login(userLogInDto);
        if (authResult.IsFailure)
            return Result<AuthResult>.Failure(
                authResult.Errors?.FirstOrDefault() ?? "Invalid login attempt",
                ResultStatus.Unauthorized);

        // Optional: Sync domain user state (e.g., reactivate if deactivated)
        var userId = authResult.UserId!.Value;
        var domainResult = await _userRepository.FindByUuid(userId, cancellationToken);
        if (domainResult == null)
        {
            await _identityService.Logout(new UserByIdDto { Uuid = authResult.UserId.Value });
            return Result<AuthResult>.Failure("User profile not found", ResultStatus.NotFound);
        }

        if (!domainResult.Value.IsDeleted)
        {
            // If domain user is deactivated, you might want to log out the identity user.
            await _identityService.Logout(new UserByIdDto { Uuid = authResult.UserId.Value });
            return Result<AuthResult>.Failure("Account is deactivated", ResultStatus.Forbidden);
        }

        return Result<AuthResult>.Success(authResult);
    }
    #endregion

    #region RefreshToken(RefreshTokenDto refreshTokenDto)
    /// <summary>
    /// Refreshes access token using refresh token.
    /// </summary>
    public async Task<Result<AuthResult>> RefreshToken(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.RefreshToken(refreshTokenDto);
        if (authResult.IsFailure)
            return Result<AuthResult>.Failure(
                authResult.Errors?.FirstOrDefault() ?? "Invalid refresh token",
                ResultStatus.Unauthorized);

        return Result<AuthResult>.Success(authResult);
    }
    #endregion

    #region ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    public async Task<Result<AuthResult>> ForgotPassword(ForgotPasswordDto forgotPasswordDto, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.ForgotPassword(forgotPasswordDto);
        if (authResult.IsFailure)
            return Result<AuthResult>.Failure(
                authResult.Errors?.FirstOrDefault() ?? "Invalid refresh token",
                ResultStatus.Unauthorized);

        return Result<AuthResult>.Success(authResult);
    }
    #endregion

    #region ConfirmEmail(ConfirmEmailDto confirmEmailDto)
    /// <summary>
    /// Confirms the user's email address using the token generated during registration.
    /// </summary>
    /// <param name="confirmEmailDto">Data transfer object containing required fields for confirm the user's  email address.</param>
    /// <returns>
    /// An <see cref="Result"/> indicating success or failure (e.g., invalid token, user not found).
    /// </returns>
    public async Task<Result> ConfirmEmail(ConfirmEmailDto confirmEmailDto)
    {
        if (confirmEmailDto.UserUuid == Guid.Empty || string.IsNullOrWhiteSpace(confirmEmailDto.Token))
            return Result.Fail("Invalid request");

        var authResult = await _identityService.ConfirmEmail(confirmEmailDto);
        if (authResult.IsFailure)
            return Result<AuthResult>.Failure(
                authResult.Errors?.FirstOrDefault() ?? "Invalid refresh token",
                ResultStatus.Unauthorized);

        var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDto.Token);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return Result.Fail(errors);
        }

        return Result.Ok(token: null, refreshToken: null, user.Id, user.UserName!);
    }
    #endregion


    #region Logout(UserByIdDto userByIdDto)
    /// <summary>
    /// Logs out the user (invalidates refresh token in Identity).
    /// </summary>
    public async Task<Result> LogoutAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken)
    {
        var result = await _identityService.Logout(userByIdDto);

        if (result.IsFailure)
            return Result.Failure("Logout failed", ResultStatus.BadRequest);

        return Result.Success();
    }
    #endregion

    #region GetCurrentUser(UserByIdDto userByIdDto)
    /// <summary>
    /// Gets the combined user information from both Identity and Domain.
    /// </summary>
    public async Task<Result<UserSingleDto>> GetCurrentUserAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken)
    {
        // Get identity user (via IIdentityService or directly? We assume IIdentityService has a method)
        // For simplicity, we retrieve domain user and map to DTO.
        var domainUser = await _userRepository.FindByUuid(userByIdDto.Uuid, cancellationToken);
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
