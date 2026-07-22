namespace ApplicationService.Common;

public class AuthResult
{
    public AuthResult() {}

    public AuthResult( string? token, string? refreshToken, Guid? userId, string? username)
    {
        Token = token;
        RefreshToken = refreshToken;
        UserId = userId;
        Username = username;
    }

    public string? Token { get; private set; }
    public string? RefreshToken { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }
}
