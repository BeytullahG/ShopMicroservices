using Shop.Catalog.Api.Contracts;

namespace Shop.Catalog.Api.Services;

public interface ICatalogReadService
{
    Task<IReadOnlyList<CategoryTreeDto>> GetCategoryTreeAsync(CancellationToken ct);
    Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductQuery query, CancellationToken ct);
    Task<ProductDetailsDto?> GetProductBySlugAsync(string Slug, CancellationToken ct);
}