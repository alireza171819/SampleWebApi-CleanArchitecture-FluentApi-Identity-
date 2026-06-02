using ApplicationService.Dtos.Authentications;
using FluentValidation;

namespace ApplicationService.Validators.Authentications;

public class ChangePasswordDtoValidator
    : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.UserUuid)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .NotEqual(x => x.CurrentPassword);
    }
}
