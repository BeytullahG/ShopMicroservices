namespace Shop.Catalog.Api.Entities;

public class Product
{
    public Guid Id {get;set;}
    public string Name {get;set;} = string.Empty;
    public string Slug {get;set;} = string.Empty;
    public string Description {get;set;} = string.Empty;
    public decimal Price {get;set;}
    public int Stock {get;set;}
    public bool IsActive{get;set;} = true;
    public Guid CategoryId {get;set;}
    public Category Category {get;set;} = null!;
    public List<ProductImage> Images {get;set;} = [];
    public DateTimeOffset CreatedAt {get;set;}
    public DateTimeOffset UpdatedAt {get;set;}


}