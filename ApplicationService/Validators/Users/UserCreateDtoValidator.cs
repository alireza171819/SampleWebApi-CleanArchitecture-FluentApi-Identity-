using ApplicationService.Dtos.Users;
using FluentValidation;

namespace ApplicationService.Validators.Users;

public class UserCreateDtoValidator
 : AbstractValidator<UserCreateDto>
{
    public UserCreateDtoValidator()
    {
        RuleFor(x => x.Uuid)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
