using Microsoft.AspNetCore.Mvc;

using Shop.Catalog.Api.Services;

namespace Shop.Catalog.Api.Controllers;

[ApiController]
[Route("categories")]
public class CategoriesController(ICatalogReadService readService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTree(CancellationToken ct) =>
        Ok(await readService.GetCategoryTreeAsync(ct));
}