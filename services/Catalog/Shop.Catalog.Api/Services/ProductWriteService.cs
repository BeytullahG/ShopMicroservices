using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

using Npgsql;

using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;
using Shop.Catalog.Api.Data;
using Shop.Catalog.Api.Entities;

namespace Shop.Catalog.Api.Services;

public class ProductWriteService(CatalogDbContext db) : IProductWtiteService
{
    private const int SlugMaxLength = 250;

    public async Task<Result<CreatedResponse>> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        if(!await db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct))
        {
            return Error.Validation(
                "Category not found",
                "Category doesn`t exist");
        }

        var slug = await SlugGenerator.MakeUniqueAsync(
            SlugGenerator.Generate(request.Name),
            SlugMaxLength,
            candidate => db.Products.AnyAsync(p => p.Slug == candidate, ct));

        var product = new Product
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId
        };
        db.Products.Add(product);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch(DbUpdateException ex) when (ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            return Error.Conflict(
                "slug_taken",
                "product with the same slug was just created, retry");
        }

        return new CreatedResponse(product.Id, product.Slug);
        
    }


    public async Task<Result> UpdateAsync(Guid Id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(
            p => p.Id == Id, ct);
        
        if (product is null)
        {
            return Error.NotFound("Product not found");
        }

        if (product.CategoryId != request.CategoryId && !await db.Categories.AnyAsync(p => p.Id == request.CategoryId, ct))
        {
            return Error.Validation(
                "category not found",
                "category doesn`t exist");
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(Guid Id, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == Id, ct);

        if (product is null)
        {
            return Error.NotFound("product not found");
        }

        product.IsActive = false;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}