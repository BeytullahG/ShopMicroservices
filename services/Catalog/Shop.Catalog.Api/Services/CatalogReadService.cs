using AutoMapper;
using AutoMapper.QueryableExtensions;

using Microsoft.EntityFrameworkCore;
using Shop.Catalog.Api.Contracts;
using Shop.Catalog.Api.Data;
using Shop.Catalog.Api.Entities;

namespace Shop.Catalog.Api.Services;

public class CatalogReadService(CatalogDbContext db, IMapper mapper, ICatalogCache cache) : ICatalogReadService
{

    private static readonly TimeSpan CategoriesTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan ProductTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ListTtl = TimeSpan.FromMinutes(5);
    
    public async Task<IReadOnlyList<CategoryTreeDto>> GetCategoryTreeAsync(CancellationToken ct)
    {
        var rows = await GetCategoryRowsAsync(ct);
        var childrenOf = rows.ToLookup(r => r.ParentId);

        List<CategoryTreeDto> Build(Guid? parentId) =>
            childrenOf[parentId]
                .Select(r => new CategoryTreeDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Slug = r.Slug,
                    Children = Build(r.Id)
                })
                .ToList();
        return Build(null);
    }

    public async Task<PagedResult<ProductListItemDto>> GetProductsAsync(ProductQuery query, CancellationToken ct)
    {
        var version = await cache.GetProductListVersionAsync(ct);

        return await cache.GetOrCreateAsync(CacheKeys.ProductList(version, query), ListTtl, token => LoadProductsAsync(query, token), ct);

        
    }

    private async Task<PagedResult<ProductListItemDto>> LoadProductsAsync(ProductQuery query, CancellationToken ct)
    {
        IQueryable<Product> q = db.Products
            .AsNoTracking()
            .Where(r => r.IsActive);
        
        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var categoryIds = await GetCategoryWithDescendantsAsync(
                query.Category, ct);
            if (categoryIds.Count == 0)
                {
                    return new PagedResult<ProductListItemDto>(
                        [], query.Page, query.PageSize, 0);    
                }
            q = q.Where(p => categoryIds.Contains(p.CategoryId));
        }
        if (query.MinPrice is { } min)   
            q = q.Where(p => p.Price >= min);
        if (query.MaxPrice is { } max)
            q = q.Where(p => p.Price <= max);
        if (query.InStock == true)
            q = q.Where(p => p.Stock > 0);
        
        var total = await q.CountAsync(ct);
        
        q = query.Sort switch
        {
            ProductSort.PriceAsc =>
                q.OrderBy(p => p.Price).ThenBy(p => p.Id),
            ProductSort.PriceDesc =>
                q.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            ProductSort.NameAsc => 
                q.OrderBy(p => p.Name).ThenBy(p => p.Id),
            _ =>
                q.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id),
        };

        var items = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<ProductListItemDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
        
        return new PagedResult<ProductListItemDto>(items, query.Page, query.PageSize, total);
    }

    private Task<List<CategoryRow>> GetCategoryRowsAsync(CancellationToken ct)
    {
        return cache.GetOrCreateAsync(
            CacheKeys.CategoryRows,
            CategoriesTtl,
            LoadCategoryRowsAsync,
            ct);
    }

    private async Task<ProductDetailsDto?> LoadProductBySlugAsync(
    string slug,
    CancellationToken ct)
{
    return await db.Products
        .AsNoTracking()
        .Where(p => p.IsActive && p.Slug == slug)
        .ProjectTo<ProductDetailsDto>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(ct);
}

    public Task<ProductDetailsDto?> GetProductBySlugAsync(string slug, CancellationToken ct) =>
        cache.GetOrCreateAsync(
            CacheKeys.Product(slug),
            ProductTtl,
            token => LoadProductBySlugAsync(slug, token),
            ct);



    // Вспомогательные методы
    
    private Task<List<CategoryRow>> LoadCategoryRowsAsync(CancellationToken ct) =>
        db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryRow(
                c.Id,
                c.ParentId,
                c.Name,
                c.Slug))
            .ToListAsync(ct);
        

    private async Task<List<Guid>> GetCategoryWithDescendantsAsync(string slug, CancellationToken ct)
    {
        var rows = await GetCategoryRowsAsync(ct);
        var root = rows.FirstOrDefault(r => r.Slug == slug);
        if(root is null) return [];

        var childrenOf = rows.ToLookup(r => r.ParentId);
        var result = new List<Guid>();
        var stack = new Stack<Guid>();
        stack.Push(root.Id);

        while(stack.Count > 0)
        {
            var id = stack.Pop();
            result.Add(id);
            foreach (var child in childrenOf[id])
            {
                stack.Push(child.Id);
            }
        }
        return result;
    }
}