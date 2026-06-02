using ApplicationService.Dtos.Authentications;
using FluentValidation;

namespace ApplicationService.Validators.Authentications;

public class UserLogInDtoValidator
    : AbstractValidator<UserLogInDto>
{
    public UserLogInDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
