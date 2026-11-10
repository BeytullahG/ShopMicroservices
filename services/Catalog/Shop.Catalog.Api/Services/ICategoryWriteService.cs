using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;

namespace Shop.Catalog.Api.Services;

public interface ICategoryWriteService
{
    Task<Result<CreatedResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken ct);
    Task<Result> UpdateAsync(Guid Id, UpdateCategoryRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(Guid Id, CancellationToken ct);
}