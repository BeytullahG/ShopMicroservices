using Shop.Catalog.Api.Contracts;
using FluentValidation;
using Shop.Catalog.Api.Entities;
namespace Shop.Catalog.Api.Validators;

public class ProductQueryValidator : AbstractValidator<ProductQuery>
{
    public const int MaxPageSize = 100;

    public ProductQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
        RuleFor(x => x.Sort).IsInEnum();
        RuleFor(x => x.Category).MaximumLength(120);
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);
        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice)
            .When(x => x.MaxPrice.HasValue && x.MinPrice.HasValue)
            .WithMessage("MaxPrice must be greater then or equal to MinPrice");
    }
}