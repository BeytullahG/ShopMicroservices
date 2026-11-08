using FluentValidation;
using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;

namespace Shop.Catalog.Api.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200)
            .Must(n => SlugGenerator.Generate(n).Length > 0)
            .WithMessage("Name must contains leters or digits");
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);

        RuleFor(x => x.CategoryId).NotEmpty();
            
    }

    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(4000);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
            RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CategoryId).NotEmpty();
        }
    }
}