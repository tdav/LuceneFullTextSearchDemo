using Microsoft.Extensions.Caching.Memory;

namespace FullTextSearchDemo.Services;

public class SearchCacheService : ISearchCacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public SearchCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T? GetOrCreate<T>(string key, Func<T> createItem)
    {
        return _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            return createItem();
        });
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }

    public void Clear()
    {
        // IMemoryCache doesn't have a built-in Clear method
        // In a production scenario, you might want to track keys separately
        // or use a different caching mechanism
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0);
        }
    }
}
