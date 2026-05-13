namespace ApplicationService.Common.Models;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();

    public static AuthResult Ok(string token, string refreshToken, int userId, string username) => new()
    {
        Success = true,
        Token = token,
        RefreshToken = refreshToken,
        UserId = userId,
        Username = username
    };

    public static AuthResult Fail(params string[] errors) => new()
    {
        Success = false,
        Errors = errors
    };
}
