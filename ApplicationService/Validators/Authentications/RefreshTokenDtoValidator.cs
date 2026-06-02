using ApplicationService.Dtos.Authentications;
using FluentValidation;

namespace ApplicationService.Validators.Authentications;

public class RefreshTokenDtoValidator
    : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
