
using Domain.Common;
using Domain.Exceptions;

namespace Domain.Aggregates.Users;

public class User : SoftDeletableEntity
{
    private User() { } // for EF

    public User(string userName, string email, bool isActive)
    {
        SetUserName(userName);
        SetEmail(email);
        IsActive = isActive;
    }
    public string Username { get; private set; }
    public string Email { get; private set; } 
    public bool IsActive { get; private set; } 

    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    public void SetUserName(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username cannot be null or empty.");
        if (username.Length < 3 || username.Length > 50)
            throw new DomainException("Username must be between 3 and 50 characters.");

        Username = username;
        Touch();
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format.");

        Email = email;
        Touch();
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        Touch();
    }

    public void SetRefreshToken(string refreshToken, DateTime expiry)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new DomainException("Refresh token cannot be empty.");
        if (expiry <= DateTime.UtcNow)
            throw new DomainException("Refresh token expiry must be in the future.");

        RefreshToken = refreshToken;
        RefreshTokenExpiry = expiry;
        Touch();
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
        Touch();
    }

    public bool IsRefreshTokenValid(string refreshToken)
    {
        return !string.IsNullOrWhiteSpace(refreshToken) &&
               RefreshToken == refreshToken &&
               RefreshTokenExpiry > DateTime.UtcNow;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
