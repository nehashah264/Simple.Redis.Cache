using Microsoft.Extensions.DependencyInjection;

namespace Simple.Redis.Cache.Distributed;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Domain
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="endpoint"></param>
    /// <param name="password"></param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddDistributedCache(this IServiceCollection services, string endpoint, string password)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = string.IsNullOrWhiteSpace(password) ? endpoint : $"{endpoint},password={password}";
        });
        return services;
    }
}