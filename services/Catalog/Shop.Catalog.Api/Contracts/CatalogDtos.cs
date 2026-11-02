namespace Shop.Catalog.Api.Contracts;

public sealed class CategoryTreeDto
{
    public Guid Id {get; init;}
    public string Name {get; init;} = string.Empty;
    public string Slug {get; init;} = string.Empty;
    public List<CategoryTreeDto> Children = [];
}

public sealed class ProductListItemDto
{
    public Guid Id {get; init;}
    public string Name {get; init;} = string.Empty;
    public string Slug {get; init;} = string.Empty;
    public decimal Price {get; init;}
    public bool InStock {get; init;}
    public string CategorySlug {get; init;} = string.Empty;
    public string? MainImageUrl {get; init;} 
}


public sealed class ProductDetailsDto
{
    public Guid Id {get; init;}
    public string Name {get; init;} = string.Empty;
    public string Slug {get; init;} = string.Empty;
    public string Description {get; init;} = string.Empty;
    public decimal Price {get; init;}
    public bool InStock {get; init;}
    public string CategoryName {get; init;} = string.Empty;
    public string CategorySlug {get; init;} = string.Empty;
    public List<ProductImageDto> Images {get; init;} = [];
}

public sealed class ProductImageDto
{
    public Guid Id {get; init;}
    public string SmallUrl {get; init;} = string.Empty;
    public string MediumUrl {get; init;} = string.Empty;
    public string LargeUrl {get; init;} = string.Empty;
}

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total)
{
    public int TotalPages =>
    (int)Math.Ceiling(Total/(double)PageSize);
}