using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;
using Shop.Catalog.Api.Services;
using Shop.Shared.Constants;

namespace Shop.Catalog.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(
    ICatalogReadService readService, 
    IProductWtiteService writeService, 
    IValidator<ProductQuery> queryValidator,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator
    ) : ControllerBase
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

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }
        var result = await writeService.CreateAsync(request, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetBySlug), new {slug = result.Value!.Slug})
            : this.ToProblem(result.Error); 
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }
        
        var result = await writeService.UpdateAsync(id, request, ct);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await writeService.DeactivateAsync(id, ct);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error);
    }

}