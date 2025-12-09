using Microsoft.Extensions.Caching.Memory;

namespace FullTextSearchDemo.Services;

public class SearchCacheService : ISearchCacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lock = new();

    public SearchCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T? GetOrCreate<T>(string key, Func<T> createItem)
    {
        lock (_lock)
        {
            if (!_cacheKeys.Contains(key))
            {
                _cacheKeys.Add(key);
            }
        }

        return _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
            entry.RegisterPostEvictionCallback((k, v, r, s) =>
            {
                lock (_lock)
                {
                    _cacheKeys.Remove(k.ToString()!);
                }
            });
            return createItem();
        });
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        lock (_lock)
        {
            _cacheKeys.Remove(key);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            foreach (var key in _cacheKeys.ToList())
            {
                _cache.Remove(key);
            }
            _cacheKeys.Clear();
        }
    }
}
