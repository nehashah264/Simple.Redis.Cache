using MemoryPack;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;

namespace Simple.Redis.Cache.Distributed;

public class CachingService : ICachingService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ConcurrentDictionary<object, SemaphoreSlim> _locks;

    public CachingService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
        _locks = new ConcurrentDictionary<object, SemaphoreSlim>();
    }

    public async Task<T> GetAsync<T>(object key, Func<Task<T>> fetch, TimeSpan? expiration = null, bool forceFetch = false)
    {
        var cacheEntry = await _distributedCache.GetAsync(key.ToString()!);
        T data = default!;

        if (!forceFetch && cacheEntry != null && cacheEntry.Length > 0)
        {
            return MemoryPackSerializer.Deserialize<T>(cacheEntry)!;
        }

        var semaphore = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();

        try
        {
            cacheEntry = await _distributedCache.GetAsync(key.ToString()!);

            if (forceFetch || cacheEntry == null || cacheEntry.Length == 0)
            {
                data = await fetch();
                await SetCacheAsync(key, expiration, data);
            }
        }
        finally
        {
            semaphore.Release();
        }

        return data ?? MemoryPackSerializer.Deserialize<T>(cacheEntry)!;
    }

    public async Task RemoveAsync(object key)
    {
        await _distributedCache.RemoveAsync(key.ToString()!);
    }

    public async Task RefreshAsync<T>(object key, T data, TimeSpan? expiration = null)
    {
        await SetCacheAsync(key, expiration, data);
    }

    public async Task<bool> ExistsKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _distributedCache.GetAsync(key, cancellationToken) != null;
    }

    public async Task SetCacheAsync<T>(object key, TimeSpan? expiration, T data, CancellationToken cancellationToken = default)
    {
        await _distributedCache.SetAsync(key.ToString()!, MemoryPackSerializer.Serialize(data), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration.HasValue ? expiration.Value : TimeSpan.FromMinutes(60),
        }, cancellationToken);
    }
}