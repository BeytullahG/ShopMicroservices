using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Shop.Catalog.Api.Services;
using Shop.Catalog.Api.Common;
using Shop.Shared.Constants;


namespace Shop.Catalog.Api.Controllers;

[ApiController]
[Route("products/{productId:guid}/images")]
[Authorize(Roles = Roles.Admin)]
public class ProductImagesServerController(IProductImageService imageService) : ControllerBase
{
    private const long RequestLimit = 6 * 1024 * 1024;
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(RequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = RequestLimit)]
    public async Task<IActionResult> Upload(Guid productId, IFormFile file, CancellationToken ct)
    {
        var result = await imageService.AddAsync(productId, file, ct);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToProblem(result.Error);
    }

    [HttpDelete("{imageId:guid}")]
    public async Task<IActionResult> Delete(Guid productId, Guid imageId, CancellationToken ct)
    {
        var result = await imageService.DeleteAsync(productId, imageId, ct);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status204NoContent)
            : this.ToProblem(result.Error);
    }
}