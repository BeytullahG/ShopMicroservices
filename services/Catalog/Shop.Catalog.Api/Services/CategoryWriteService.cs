using Shop.Catalog.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

using Npgsql;

using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;
using Shop.Catalog.Api.Entities;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Shop.Catalog.Api.Services;

public class CategoryWriteService(CatalogDbContext db) : ICategoryWriteService
{
    private const int SlugMaxLength = 120;

    public async Task<Result<CreatedResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken ct)
    {
        if (request.ParentId is { } parentId && !await db.Categories.AnyAsync(p => p.Id == request.ParentId, ct))
        {
            return Error.Validation(
                "parent_not_found",
                "Parent category doesn`t exist");
        }

        var slug = await SlugGenerator.MakeUniqueAsync(
            SlugGenerator.Generate(request.Name),
            SlugMaxLength,
            candidate => db.Categories.AnyAsync(c => c.Slug == candidate, ct));

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            ParentId = request.ParentId
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return new CreatedResponse(category.Id, category.Slug);
    }


    public async Task<Result> UpdateAsync(Guid Id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(p => p.Id == Id, ct);
        if (category is null)
        {
            return Error.NotFound("category not found");
        }

        if (request.ParentId != category.ParentId)
        {
            var parents = await db.Categories
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Id, c => c.ParentId, ct);

            if (request.ParentId is { } newParentId && !parents.ContainsKey(newParentId))
            {
                return Error.Validation(
                    "parent_not_found",
                    "Parent category does not exist");
            }

            if (WouldCreateCycle(Id, request.ParentId, parents))
            {
                return Error.Validation("category_cycle", "Category cannot be moved inside itself or its descendant");
            }
        }

        category.Name = request.Name.Trim();
        category.ParentId = request.ParentId;

        await db.SaveChangesAsync(ct);
        return Result.Success();

    }



    public async Task<Result> DeleteAsync(Guid Id, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(p => p.Id == Id, ct);
        if (category is null)
        {
            return Error.NotFound("category not found");
        }

        var hasChildren = await db.Categories.AnyAsync(p => p.ParentId == Id, ct);
        var hasProducts = await db.Products.AnyAsync(p => p.CategoryId == Id, ct);
        if (hasChildren || hasProducts)
        {
            return Error.Conflict("category_not_empty", "Move or remove subcategories and products first");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }



    private static bool WouldCreateCycle(Guid categoryId, Guid? newParentId, Dictionary<Guid, Guid?> parents)
    {
        var current = newParentId;
        while (current is { } currentId)
        {
            if (currentId == categoryId)
            {
                return true;
            }
            current = parents.GetValueOrDefault(currentId);
        }
        return false;
    }
}