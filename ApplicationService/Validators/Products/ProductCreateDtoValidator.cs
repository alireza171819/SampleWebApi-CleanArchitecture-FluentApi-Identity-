using ApplicationService.Dtos.Products;
using FluentValidation;

namespace ApplicationService.Validators.Products;

public class ProductCreateDtoValidator
: AbstractValidator<ProductCreateDto>
{
    public ProductCreateDtoValidator()
    {
        RuleFor(x => x.UUId)
            .NotEqual(Guid.Empty);

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0);

        RuleFor(x => x.UnitsInStock)
            .GreaterThanOrEqualTo(0);
    }
}
