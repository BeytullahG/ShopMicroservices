namespace Shop.Catalog.Api.Validators;

using System.Data;

using FluentValidation;
using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .Must(n => SlugGenerator.Generate(n).Length > 0)
            .WithMessage("Name must contain letters or digits");
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}