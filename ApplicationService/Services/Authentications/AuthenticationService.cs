using ApplicationService.Common.Models;
using ApplicationService.Dtos.Authentications;
using ApplicationService.Dtos.Users;
using ApplicationService.Services.Contracts;
using Domain.Aggregates.Users;
using Domain.Common;
using Domain.Contracts.Persistence;
using FluentValidation;

namespace ApplicationService.Services.Authentications;

public class AuthenticationService : IAuthenticationService
{
    #region Privet Fields
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<ConfirmEmailDto> _confirmEmailValidator;
    private readonly IValidator<ForgotPasswordDto> _forgotPasswordValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
    private readonly IValidator<UserLoginDto> _userLoginValidator;
    private readonly IValidator<UserRegisterDto> _userRegisterValidator;
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
    /// <param name="userRegisterDto">The data required to register a new user.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result"/> indicating whether the registration completed successfully,
    /// or containing validation and registration errors.
    /// </returns>
    public async Task<Result> RegisterAsync(UserRegisterDto userRegisterDto, CancellationToken cancellationToken)
    {
        var validationResult = await _userRegisterValidator.ValidateAsync(userRegisterDto, cancellationToken);

        if (!validationResult.IsValid)
            return Result.BadRequest(string.Join(" | ", validationResult.Errors.Select(x => x.ErrorMessage)));

        var authResult = await _identityService.RegisterAsync(userRegisterDto);
        if (authResult.IsFailure)
            return Result.Failure("Registration failed.", ResultStatus.BadRequest);

        var domainUser = new User(userRegisterDto.Username, userRegisterDto.Email);
        domainUser.SetUid(authResult.UserId!.Value); // Set the Uuid to match Identity Id

        var insertResult = await _userRepository.InsertAsync(domainUser, cancellationToken);
        if (insertResult.IsFailure)
        {
            // Compensation: rollback Identity user
            await _identityService.LogoutAsync(new UserByIdDto { Uuid = authResult.UserId.Value});
            await _identityService.DeleteUserAsync(new UserByIdDto { Uuid = authResult.UserId.Value });
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
    /// Authenticates the user and returns authentication tokens. Also synchronizes
    /// the user's domain state if required.
    /// </summary>
    /// <param name="userLogInDto">The user credentials required for authentication.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing an <see cref="AuthResult"/> on success,
    /// or authentication error details if the operation fails.
    /// </returns>
    public async Task<Result<AuthResult>> LoginAsync(UserLoginDto userLogInDto, CancellationToken cancellationToken)
    {
        if (userLogInDto == null)
            return Result<AuthResult>.BadRequest("Model is null.");

        var validationResult = await _userLoginValidator.ValidateAsync(userLogInDto, cancellationToken);

        if (!validationResult.IsValid)
            return Result<AuthResult>.BadRequest(string.Join(" | ", validationResult.Errors.Select(x => x.ErrorMessage)));

        var authResult = await _identityService.LoginAsync(userLogInDto);
        if (authResult.IsFailure)
            return Result<AuthResult>.Failure(
                authResult.Errors?.FirstOrDefault() ?? "Invalid login attempt",
                ResultStatus.Unauthorized);

        // Optional: Sync domain user state (e.g., reactivate if deactivated)
        var userId = authResult.UserId!.Value;
        var domainResult = await _userRepository.FindByUuidAsync(userId, cancellationToken);
        if (domainResult == null)
        {
            await _identityService.LogoutAsync(new UserByIdDto { Uuid = authResult.UserId.Value });
            return Result<AuthResult>.Failure("User profile not found", ResultStatus.NotFound);
        }

        if (!domainResult.Value.IsDeleted)
        {
            // If domain user is deactivated, you might want to log out the identity user.
            await _identityService.LogoutAsync(new UserByIdDto { Uuid = authResult.UserId.Value });
            return Result<AuthResult>.Failure("Account is deactivated", ResultStatus.Forbidden);
        }

        return Result<AuthResult>.Success(authResult);
    }
    #endregion

    #region RefreshToken(RefreshTokenDto refreshTokenDto)
    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">The refresh token request data.</param>
    /// <param name="cancellationToken">Token to cancel the operation (e.g., due to client disconnect or timeout).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a new <see cref="AuthResult"/> if the refresh
    /// operation succeeds; otherwise, an error result.
    /// </returns>
    public async Task<Result<AuthResult>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken)
    {
        if (refreshTokenDto == null)
            return Result<AuthResult>.BadRequest("Model is null.");

        if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
            return Result<AuthResult>.BadRequest("Invalid refresh token.");

        var authResult = await _identityService.RefreshTokenAsync(refreshTokenDto);
        if (authResult.IsFailure)
            return Result<AuthResult>.Failure(
                authResult.Errors?.FirstOrDefault() ?? "Invalid refresh token",
                ResultStatus.Unauthorized);

        return Result<AuthResult>.Success(authResult);
    }
    #endregion

    #region ForgotPassword(ForgotPasswordDto forgotPasswordDto)
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
    public async Task<Result<AuthResult>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto, CancellationToken cancellationToken)
    {
        if (forgotPasswordDto == null)
            return Result<AuthResult>.BadRequest("Model is null.");


        var validationResult = await _forgotPasswordValidator.ValidateAsync(forgotPasswordDto, cancellationToken);

        if (!validationResult.IsValid)
            return Result<AuthResult>.BadRequest(string.Join(" | ", validationResult.Errors.Select(x => x.ErrorMessage)));

        var authResult = await _identityService.ForgotPasswordAsync(forgotPasswordDto);
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
    public async Task<Result> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto, CancellationToken cancellationToken)
    {
        if (confirmEmailDto == null)
            return Result.BadRequest("Model is null.");

        var validationResult = await _confirmEmailValidator.ValidateAsync(confirmEmailDto, cancellationToken);

        if (!validationResult.IsValid)
            return Result.BadRequest(string.Join(" | ", validationResult.Errors.Select(x => x.ErrorMessage)));

        var result = await _identityService.ConfirmEmailAsync(confirmEmailDto);

        if (result.IsFailure)
            return Result.Failure(result.Errors);
    }
    #endregion

    /// <summary>
    /// Changes the user's password after verifying the current password.
    /// </summary>
    ///  <param name="changePasswordDto">Data transfer object containing required fields for change user password.</param>
    /// <returns>
    public async Task<AuthResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto, CancellationToken cancellationToken)
    {
        if (changePasswordDto == null)
            return AuthResult.Fail("Model is null.");

        var validationResult = await _changePasswordValidator.ValidateAsync(changePasswordDto, cancellationToken);

        if (!validationResult.IsValid)
            return AuthResult.Fail(validationResult.Errors.Select(e => e.ErrorMessage).ToArray());

        var result = await _identityService.ChangePasswordAsync(changePasswordDto);

        if (result.IsFailure)
        {
            var errors = result.Errors.Select(e => e.d).ToArray();
            return AuthResult.Fail(errors);
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return AuthResult.Ok(token: null, refreshToken: null, user.Id, user.UserName!);
    }


    #region Logout(UserByIdDto userByIdDto)
    /// <summary>
    /// Logs out the user (invalidates refresh token in Identity).
    /// </summary>
    public async Task<Result> LogoutAsync(UserByIdDto userByIdDto, CancellationToken cancellationToken)
    {
        var result = await _identityService.LogoutAsync(userByIdDto);

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
        var domainUser = await _userRepository.FindByUuidAsync(userByIdDto.Uuid, cancellationToken);
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
