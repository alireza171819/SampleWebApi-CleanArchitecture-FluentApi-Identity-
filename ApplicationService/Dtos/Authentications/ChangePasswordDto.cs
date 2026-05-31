
namespace ApplicationService.Dtos.Authentications;

public class ChangePasswordDto
{
    public Guid UserUuid { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
