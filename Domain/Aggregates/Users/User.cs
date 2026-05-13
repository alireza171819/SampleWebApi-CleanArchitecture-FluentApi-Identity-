
using Domain.Common;
using Domain.Exceptions;

namespace Domain.Aggregates.Users;

public class User : SoftDeletableEntity
{
    private User() { } // for EF

    public User(string userName, string email, bool isActive)
    {
        SetUserName(userName); 
        Email = email;
        IsActive = isActive;
    }
    public string Username { get; private set; }
    public string Email { get; private set; } 
    public bool IsActive { get; private set; } 

    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    public void SetUserName(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            throw new DomainException("User name is required.");

        Username = userName;
        Touch();
    }
}
