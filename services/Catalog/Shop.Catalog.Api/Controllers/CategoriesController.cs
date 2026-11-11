using System.ComponentModel.DataAnnotations;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;
using Shop.Catalog.Api.Services;
using Shop.Shared.Constants;

namespace Shop.Catalog.Api.Controllers;

[ApiController]
[Route("categories")]
public class CategoriesController(
    ICatalogReadService readService,
    ICategoryWriteService writeService,
    IValidator<CreateCategoryRequest> createValidator,
    IValidator<UpdateCategoryRequest> updateValidator
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTree(CancellationToken ct) =>
        Ok(await readService.GetCategoryTreeAsync(ct));

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }
        var result = await writeService.CreateAsync(request, ct);
        return result.IsSuccess 
        ? StatusCode(StatusCodes.Status201Created, result.Value)
        : this.ToProblem(result.Error);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return ValidationProblem(ModelState);
        }
        var result = await writeService.UpdateAsync(id, request, ct);
        return result.IsSuccess
        ? StatusCode(StatusCodes.Status204NoContent)
        : this.ToProblem(result.Error);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await writeService.DeleteAsync(id, ct);
        return result.IsSuccess
        ? NoContent() : this.ToProblem(result.Error);
    }
}