
namespace ApplicationService.Dtos.Authentications;

public class ConfirmEmailDto
{
    public Guid UserUuid { get; set; }
    public string Token { get; set; }
}
