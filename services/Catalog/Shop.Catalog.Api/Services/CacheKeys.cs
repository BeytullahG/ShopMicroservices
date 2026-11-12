using System.Globalization;

using Shop.Catalog.Api.Common;
using Shop.Catalog.Api.Contracts;

namespace Shop.Catalog.Api.Services;

public static class CacheKeys
{
    public const string CategoryRows = "categories:rows";
    public const string ProductListVersion = "products:list:version";
    public static string Product(string slug) => $"product:slug:{slug}";
    public static string ProductList(string version, ProductQuery q) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"products:list:{version}:{q.Category?.ToLowerInvariant()}:{q.MinPrice}:{q.MaxPrice}:{q.InStock}:{q.Sort}:{q.Page}:{q.PageSize}");
}