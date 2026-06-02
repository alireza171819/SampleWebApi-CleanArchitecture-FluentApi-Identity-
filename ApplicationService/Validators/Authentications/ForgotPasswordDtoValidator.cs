using ApplicationService.Dtos.Authentications;
using FluentValidation;

namespace ApplicationService.Validators.Authentications;

public class ForgotPasswordDtoValidator
    : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
