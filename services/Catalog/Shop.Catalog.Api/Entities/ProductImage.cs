namespace Shop.Catalog.Api.Entities;

public class ProductImage
{
    public Guid Id {get;set;}

    public Guid ProductId {get;set;}
    public Product Product {get;set;} = null!;

    public string FileName {get;set;} = string.Empty;
    public int SortOrder{get;set;}
}