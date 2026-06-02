using ApplicationService.Dtos.Users;
using FluentValidation;

namespace ApplicationService.Validators.Users;

public class UserUpdateDtoValidator
: AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

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
