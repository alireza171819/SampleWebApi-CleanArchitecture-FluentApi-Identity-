using ApplicationService.Dtos.Orders;
using FluentValidation;

namespace ApplicationService.Validators.Orders;

public class OrderDetailSingleDtoValidator
 : AbstractValidator<OrderDetailSingleDto>
{
    public OrderDetailSingleDtoValidator()
    {
        RuleFor(x => x.UUId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0);
    }
}
