
using Microsoft.AspNetCore.Identity;

namespace Identity.Entities;

public class AppUser : IdentityUser<Guid>
{
    public AppUser()
    {
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        IsDelete = false;
    }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } 
    public bool IsActive { get; set; }
    public bool IsDelete { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

}
