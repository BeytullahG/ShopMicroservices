using System.Reflection.Metadata.Ecma335;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;
using Shop.Catalog.Api.Services;

namespace Shop.Catalog.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(ICatalogReadService readService, IValidator<ProductQuery> queryValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] ProductQuery query,
        CancellationToken ct
    )
    {
        var validation = await queryValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }
        return Ok(await readService.GetProductsAsync(query, ct));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var product = await readService.GetProductBySlugAsync(slug, ct);
        return product is null ? NotFound() : Ok(product);
    }

}