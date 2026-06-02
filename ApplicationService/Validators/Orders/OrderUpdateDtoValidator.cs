using ApplicationService.Dtos.Orders;
using FluentValidation;

namespace ApplicationService.Validators.Orders;

public class OrderUpdateDtoValidator
    : AbstractValidator<OrderUpdateDto>
{
    public OrderUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.UUId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.UserId)
            .GreaterThan(0);

        RuleFor(x => x.ShipAddress)
            .NotEmpty();

        RuleFor(x => x.ShipedDate)
            .GreaterThanOrEqualTo(x => x.OrderDate);

        RuleForEach(x => x.SingleOrderDetailDtos)
            .SetValidator(new OrderDetailSingleDtoValidator());
    }
}
