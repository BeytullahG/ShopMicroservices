using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Shop.Catalog.Api.Contracts;

public sealed record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    Guid CategoryId);

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    bool IsActive);

public sealed record CreateCategoryRequest(string Name, Guid? ParentId);
public sealed record UpdateCategoryRequest(string Name, Guid? ParentId);
public sealed record CreatedResponse(Guid Id, string Slug);