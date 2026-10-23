using System.Net.Http.Headers;

namespace Shop.Catalog.Api.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? PublicId { get; set; }
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = [];
    public List<Product> Products{get;set;} = [];

}