namespace ApplicationService.Dtos.Users;

public class UserUpdateDto
{
    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}
