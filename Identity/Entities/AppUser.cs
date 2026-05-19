
using Microsoft.AspNetCore.Identity;

namespace Identity.Entities;

public class AppUser : IdentityUser<Guid>
{
    public AppUser()
    {
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } 
    public bool IsActive { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

}
