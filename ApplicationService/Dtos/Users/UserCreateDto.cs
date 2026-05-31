
namespace ApplicationService.Dtos.Users;

public class UserCreateDto
{
    public Guid Uuid { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool IsEmailConfirmed { get; set; }

}
