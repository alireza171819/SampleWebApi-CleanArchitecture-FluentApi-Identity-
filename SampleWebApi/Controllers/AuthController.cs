using ApplicationService.Dtos.Authentications;
using ApplicationService.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SampleWebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        UserRegisterDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RegisterAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        UserLoginDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        RefreshTokenDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RefreshTokenAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        ConfirmEmailDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.ConfirmEmailAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.ForgotPasswordAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.ResetPasswordAsync(dto, cancellationToken);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        // معمولاً logout نیاز به دریافت توکن از هدر دارد
        var result = await _authenticationService.LogoutAsync(User, cancellationToken);
        return HandleResult(result);
    }
}