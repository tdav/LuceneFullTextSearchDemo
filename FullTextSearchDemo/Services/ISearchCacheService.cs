namespace FullTextSearchDemo.Services;

public interface ISearchCacheService
{
    T? GetOrCreate<T>(string key, Func<T> createItem);
    
    void Remove(string key);
    
    void Clear();
}
