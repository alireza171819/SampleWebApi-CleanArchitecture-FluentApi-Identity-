using ApplicationService.Dtos.Orders;
using FluentValidation;

namespace ApplicationService.Validators.Orders;

public class OrderDetailSingleDtoValidator
 : AbstractValidator<OrderDetailSingleDto>
{
    public OrderDetailSingleDtoValidator()
    {
        RuleFor(x => x.Uuid).NotEqual(Guid.Empty).WithMessage("Uuid is required.");

        RuleFor(x => x.ProductId).GreaterThanOrEqualTo(0).WithMessage("Product identifier is required.");

        RuleFor(x => x.Quantity).NotNull().GreaterThan(0).WithMessage("Quantity is invalid.");

        RuleFor(x => x.UnitPrice).NotNull().GreaterThan(0).WithMessage("Unit price is invalid.");
    }
}
