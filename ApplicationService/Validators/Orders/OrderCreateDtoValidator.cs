using ApplicationService.Dtos.Orders;
using FluentValidation;

namespace ApplicationService.Validators.Orders;

public class OrderCreateDtoValidator
    : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator()
    {
        RuleFor(x => x.UserId).NotNull().GreaterThan(0).WithMessage("User identifier is Required.");

        RuleFor(x => x.OrderDate).NotNull().WithMessage("Order date is invalid.");

        RuleFor(x => x.ShippingAddress).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Ship address is required.")
           .MaximumLength(500).WithMessage("The address cannot be longer than 500 words.");

        RuleFor(x => x.ShippedDate).Must((dto, shipDate) => !shipDate.HasValue || shipDate.Value >= dto.OrderDate).WithMessage("Shipped date cannot be before OrderDate.");

        RuleFor(x => x.OrderDetails).NotNull().WithMessage("Order details cannot be null.");

        RuleForEach(x => x.OrderDetails).SetValidator(new OrderDetailSingleDtoValidator());
    }
}
