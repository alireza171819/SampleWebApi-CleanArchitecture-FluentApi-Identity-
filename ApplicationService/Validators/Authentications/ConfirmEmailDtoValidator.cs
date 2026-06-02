using ApplicationService.Dtos.Authentications;
using FluentValidation;

namespace ApplicationService.Validators.Authentications;

public class ConfirmEmailDtoValidator
    : AbstractValidator<ConfirmEmailDto>
{
    public ConfirmEmailDtoValidator()
    {
        RuleFor(x => x.UserUuid)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.Token)
            .NotEmpty();
    }
}
