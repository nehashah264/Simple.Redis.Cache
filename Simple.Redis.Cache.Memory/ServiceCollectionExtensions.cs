using Microsoft.Extensions.DependencyInjection;

namespace Simple.Redis.Cache.Memory;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryCache(this ServiceCollection services)
    {
        services.AddTransient<ICachingService, CachingService>();
        return services;
    }
}