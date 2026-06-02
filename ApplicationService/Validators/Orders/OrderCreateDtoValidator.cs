using ApplicationService.Dtos.Orders;
using FluentValidation;

namespace ApplicationService.Validators.Orders;

public class OrderCreateDtoValidator
    : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator()
    {
        RuleFor(x => x.Uuid)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.ShipAddress)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.ShipedDate)
            .GreaterThanOrEqualTo(x => x.OrderDate);

        RuleFor(x => x.OrderDetailsDtos)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.OrderDetailsDtos)
            .SetValidator(new OrderDetailSingleDtoValidator());
    }
}
