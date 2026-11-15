using System.Diagnostics.Tracing;
using System.Text.Json;
using System.Threading.Tasks.Sources;

using Microsoft.Extensions.Caching.Distributed;

namespace Shop.Catalog.Api.Services;

public interface ICatalogCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken ct);

    Task RemoveAsync(string key, CancellationToken ct);
    Task<string> GetProductListVersionAsync(CancellationToken ct);
    Task BumpProductListVersionAsync(CancellationToken ct);
}


public sealed class CatalogCache(IDistributedCache cache, ILogger<CatalogCache> logger) : ICatalogCache
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken ct)
    {
        var cached = await TryGetAsync(key, ct);

        if(cached is not null)
        {
            return JsonSerializer.Deserialize<T>(cached, Json)!;
        }
        var value = await factory(ct);

        if(value is not null)
        {
            await TrySetAsync(key, JsonSerializer.Serialize(value, Json), ttl, ct);
        }
        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        try
        {
            await cache.RemoveAsync(key, ct);
        }
        catch(Exception ex) when(ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Redis remove failed for {Key}", key);
        }
    }

    public async Task<string> GetProductListVersionAsync(CancellationToken ct)
    {
        var version = await TryGetAsync(CacheKeys.ProductListVersion, ct);
        if(version is not null)
        {
            return version;
        }

        version = Guid.NewGuid().ToString("N");

        await TrySetAsync(CacheKeys.ProductListVersion, version, ttl: null, ct);

        return version;
        
    }


    public Task BumpProductListVersionAsync(CancellationToken ct) =>
        TrySetAsync(CacheKeys.ProductListVersion, Guid.NewGuid().ToString("N"), ttl: null, ct);
    


    private async Task<string?> TryGetAsync(string key, CancellationToken ct)
    {
        try
        {
            return await cache.GetStringAsync(key, ct);
        }
        catch(Exception ex) when(ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Redis read failed for {Key}, falling back to database", key);
            return null;
        }
    }

    private async Task TrySetAsync(string key, string value, TimeSpan? ttl, CancellationToken ct)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            await cache.SetStringAsync(key, value, options, ct);
        }
        catch(Exception ex) when(ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Redis write failed for {Key}", key);
        }
    }
}