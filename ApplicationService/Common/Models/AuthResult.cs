
namespace ApplicationService.Common.Models;

public class AuthResult
{
    public AuthResult()
    {
        Errors = Array.Empty<string>();
    }

    public AuthResult(bool isSuccess, string? token, string? refreshToken, Guid? userId, string? username, string[] errors)
    {
        IsSuccess = isSuccess;
        Token = token;
        RefreshToken = refreshToken;
        UserId = userId;
        Username = username;
        Errors = errors;
    }

    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string? Token { get; private set; }
    public string? RefreshToken { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }
    public string[] Errors { get; private set; } 

    public static AuthResult Ok(string token, string refreshToken, Guid userId, string username) => new(true, token, refreshToken, userId, username, Array.Empty<string>());

    public static AuthResult Fail(params string[] errors) => new(false, null, null, Guid.Empty, null, errors);
}
