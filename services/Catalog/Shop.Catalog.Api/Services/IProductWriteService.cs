using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;

namespace Shop.Catalog.Api.Services;

public interface IProductWtiteService
{
    Task<Result<CreatedResponse>> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<Result> UpdateAsync(Guid Id, UpdateProductRequest request, CancellationToken ct);
    Task<Result> DeactivateAsync(Guid Id, CancellationToken ct);
    

}