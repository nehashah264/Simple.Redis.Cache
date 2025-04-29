namespace Simple.Redis.Cache;

/// <summary>
/// Caching Service Interface
/// </summary>
public interface ICachingService
{
    /// <summary>
    /// Gets an object async from the cache if exists or executes the fetch delegate and cache
    /// </summary>
    /// <param name="key">Cache Key</param>
    /// <param name="fetch">Fetch function</param>
    /// <param name="expiration">When cache expires from now</param>
    /// <param name="forceFetch">Forces the fetch execution when true</param>
    /// <typeparam name="T">Data Type to be cached</typeparam>
    /// <returns>Cached data of type T</returns>
    Task<T> GetAsync<T>(object key, Func<Task<T>> fetch, TimeSpan? expiration = null, bool forceFetch = false);

    /// <summary>
    /// Removes the object from the cache with the selected key
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns></returns>
    Task RemoveAsync(object key);

    Task RefreshAsync<T>(object key, T data, TimeSpan? expiration = null);

    Task<bool> ExistsKeyAsync(string key, CancellationToken cancellationToken = default);

    Task SetCacheAsync<T>(object key, TimeSpan? expiration, T data, CancellationToken cancellationToken = default);
}