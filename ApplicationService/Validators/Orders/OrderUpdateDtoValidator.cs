using ApplicationService.Dtos.Orders;
using FluentValidation;

namespace ApplicationService.Validators.Orders;

public class OrderUpdateDtoValidator
    : AbstractValidator<OrderUpdateDto>
{
    public OrderUpdateDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThanOrEqualTo(0).NotNull().WithMessage("Order identifier is required.");

        RuleFor(x => x.UserId).NotNull().GreaterThanOrEqualTo(0).WithMessage("User identifier is Required.");

        RuleFor(x => x.ShippingAddress).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Shipping address is required.")
            .MaximumLength(500).WithMessage("The address cannot be longer than 500 words.");

        RuleFor(x => x.ShippedDate).Must((dto, shipDate) => !shipDate.HasValue || shipDate.Value >= dto.OrderDate).WithMessage("Shipped date cannot be before OrderDate.");

        RuleForEach(x => x.OrderDetails).SetValidator(new OrderDetailSingleDtoValidator());
    }
}
