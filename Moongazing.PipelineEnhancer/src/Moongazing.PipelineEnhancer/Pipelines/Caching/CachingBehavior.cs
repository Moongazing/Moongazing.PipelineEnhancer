using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Moongazing.PipelineEnhancer.Pipelines.Caching;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICachableRequest
{
    private readonly CacheSettings cacheSettings;
    private readonly IDistributedCache cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> logger;

    public CachingBehavior(IDistributedCache cache,
                           ILogger<CachingBehavior<TRequest, TResponse>> logger,
                           IConfiguration configuration)
    {
        this.cache = cache;
        this.logger = logger;
        cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? throw new InvalidOperationException();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.BypassCache)
        {
            return await next();
        }

        TResponse response;

        byte[]? cachedResponse = await cache.GetAsync(request.CacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            var cachedString = Encoding.Default.GetString(cachedResponse);

            response = JsonSerializer.Deserialize<TResponse>(cachedString) ?? throw new InvalidOperationException($"Deserialization failed for cache key {request.CacheKey}");

            logger.LogInformation("Fetched from Cache -> {CacheKey}", request.CacheKey);
        }
        else
        {
            response = await GetResponseAndAddToCache(request, next, cancellationToken);
        }

        return response;
    }

    private async Task<TResponse> GetResponseAndAddToCache(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next();

        TimeSpan slidingExpiration = request.SlidingExpiration ?? TimeSpan.FromDays(cacheSettings.SlidingExpiration);

        DistributedCacheEntryOptions cacheOptions = new() { SlidingExpiration = slidingExpiration };

        byte[] serializedData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

        await cache.SetAsync(request.CacheKey, serializedData, cacheOptions, cancellationToken);

        logger.LogInformation("Added to Cache -> {CacheKey}", request.CacheKey);

        if (request.CacheGroupKey != null)
        {
            await AddCacheKeyToGroup(request, slidingExpiration, cancellationToken);
        }

        return response;
    }

    private async Task AddCacheKeyToGroup(TRequest request, TimeSpan slidingExpiration, CancellationToken cancellationToken)
    {
        byte[]? cacheGroupCache = await cache.GetAsync(request.CacheGroupKey!, cancellationToken);

        HashSet<string> cacheKeysInGroup;

        if (cacheGroupCache != null)
        {
            cacheKeysInGroup = JsonSerializer.Deserialize<HashSet<string>>(Encoding.Default.GetString(cacheGroupCache)) ?? [];

            if (!cacheKeysInGroup.Contains(request.CacheKey))
            {
                cacheKeysInGroup.Add(request.CacheKey);
            }
        }
        else
        {
            cacheKeysInGroup = [request.CacheKey];
        }

        byte[] newCacheGroupCache = JsonSerializer.SerializeToUtf8Bytes(cacheKeysInGroup);

        byte[]? cacheGroupCacheSlidingExpirationCache = await cache.GetAsync($"{request.CacheGroupKey}SlidingExpiration", cancellationToken);

        int? cacheGroupCacheSlidingExpirationValue = null;

        if (cacheGroupCacheSlidingExpirationCache != null)
        {
            _ = int.TryParse(Encoding.Default.GetString(cacheGroupCacheSlidingExpirationCache), out int parsedValue);
            cacheGroupCacheSlidingExpirationValue = parsedValue;
        }

        if (cacheGroupCacheSlidingExpirationValue == null || slidingExpiration.TotalSeconds > cacheGroupCacheSlidingExpirationValue)
        {
            cacheGroupCacheSlidingExpirationValue = (int)slidingExpiration.TotalSeconds;
        }

        byte[] serializeCachedGroupSlidingExpirationData = JsonSerializer.SerializeToUtf8Bytes(cacheGroupCacheSlidingExpirationValue.Value);

        DistributedCacheEntryOptions cacheOptions = new() { SlidingExpiration = TimeSpan.FromSeconds(cacheGroupCacheSlidingExpirationValue.Value) };

        await cache.SetAsync(request.CacheGroupKey!, newCacheGroupCache, cacheOptions, cancellationToken);

        logger.LogInformation("Added to Cache -> {CacheGroupKey}", request.CacheGroupKey);

        await cache.SetAsync($"{request.CacheGroupKey}SlidingExpiration", serializeCachedGroupSlidingExpirationData, cacheOptions, cancellationToken);

        logger.LogInformation("Added to Cache -> {CacheGroupKey} SlidingExpiration", request.CacheGroupKey);
    }
}
