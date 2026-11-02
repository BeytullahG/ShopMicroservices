namespace Shop.Catalog.Api.Contracts;

public enum ProductSort
{
    Newest, 
    PriceAsc,
    PriceDesc,
    NameAsc
}

public sealed class ProductQuery
{
    public string? Category {get; set;}
    public decimal? MaxPrice {get; set;}
    public decimal? MinPrice {get; set;}
    public bool? InStock {get; set;}
    public ProductSort Sort {get; set;} = ProductSort.Newest;
    public int Page {get; set;} = 1;
    public int PageSize {get; set;} = 20;

    
}


