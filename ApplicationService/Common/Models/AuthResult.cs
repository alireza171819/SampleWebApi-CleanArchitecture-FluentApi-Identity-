namespace ApplicationService.Common.Models;

public class AuthResult
{
    public AuthResult()
    {
        Errors = Array.Empty<string>();
    }
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string? Token { get; private set; }
    public string? RefreshToken { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }
    public string[] Errors { get; private set; } 

    public static AuthResult Ok(string token, string refreshToken, Guid userId, string username) => new()
    {
        IsSuccess = true,
        Token = token,
        RefreshToken = refreshToken,
        UserId = userId,
        Username = username
    };

    public static AuthResult Fail(params string[] errors) => new()
    {
        IsSuccess = false,
        Errors = errors
    };
}
