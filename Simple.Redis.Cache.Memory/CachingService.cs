using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Simple.Redis.Cache.Memory;

/// <summary>
/// In-memory Caching Service
/// </summary>
public class CachingService : ICachingService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _locks;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="memoryCache">Memory Cache</param>
    public CachingService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _locks = new ConcurrentDictionary<object, SemaphoreSlim>();
    }

    /// <summary>
    /// Gets an object asynchronously from the cache if exists or executes the fetch delegate and cache
    /// </summary>
    /// <param name="key">Cache Key</param>
    /// <param name="fetch">Fetch function</param>
    /// <param name="expiration">When cache expires from now</param>
    /// <param name="forceFetch">Forces the fetch execution when true</param>
    /// <typeparam name="T">Data Type to be cached</typeparam>
    /// <returns>Cached data of type T</returns>
    public async Task<T> GetAsync<T>(object key, Func<Task<T>> fetch, TimeSpan? expiration = null, bool forceFetch = false)
    {
        if (!forceFetch && _memoryCache.TryGetValue(key, out T? cacheEntry) && cacheEntry != null)
        {
            return cacheEntry;
        }

        var semaphore = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();

        try
        {
            var existsCache = _memoryCache.TryGetValue(key, out cacheEntry);

            if (forceFetch || !existsCache || cacheEntry == null)
            {
                cacheEntry = await fetch();
                SetCache(key, expiration, cacheEntry);
            }
        }
        finally
        {
            semaphore.Release();
        }

        return cacheEntry;
    }

    /// <summary>
    /// Removes the object from the cache with the selected key
    /// </summary>
    /// <param name="key">Cache key</param>
    public async Task RemoveAsync(object key)
    {
        var semaphore = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();

        try
        {
            _memoryCache.Remove(key);
        }
        finally { semaphore.Release(); }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="expiration"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task RefreshAsync<T>(object key, T data, TimeSpan? expiration = null)
    {
        var semaphore = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();

        try
        {
            SetCache(key, expiration, data);
        }
        finally { semaphore.Release(); }
    }

    public Task<bool> ExistsKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    public Task SetCacheAsync<T>(object key, TimeSpan? expiration, T data, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => SetCache(key, expiration, data), cancellationToken);
    }

    private void SetCache<T>(object key, TimeSpan? expiration, T cacheEntry)
    {
        if (expiration.HasValue)
        {
            _memoryCache.Set(key, cacheEntry, expiration.Value);
        }
        else
        {
            _memoryCache.Set(key, cacheEntry);
        }
    }
}