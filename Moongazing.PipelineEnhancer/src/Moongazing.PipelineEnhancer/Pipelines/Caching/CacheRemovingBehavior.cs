using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace Moongazing.PipelineEnhancer.Pipelines.Caching;

public class CacheRemovingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheRemoverRequest
{
    private readonly IDistributedCache cache;
    public CacheRemovingBehavior(IDistributedCache cache)
    {
        this.cache = cache;
    }


    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.BypassCache)
        {
            return await next();
        }

        TResponse response = await next();

        if (request.CacheGroupKey != null)
        {
            byte[]? cachedGroup = await cache.GetAsync(request.CacheGroupKey, cancellationToken);

            if (cachedGroup != null)
            {
                HashSet<string> keysInGroup = JsonSerializer.Deserialize<HashSet<string>>(Encoding.Default.GetString(cachedGroup))!;

                foreach (string key in keysInGroup)
                {
                    await cache.RemoveAsync(key, cancellationToken);
                }

                await cache.RemoveAsync(request.CacheGroupKey, cancellationToken);

                await cache.RemoveAsync(key: $"{request.CacheGroupKey}SlidingExpiration", cancellationToken);
            }
        }

        if (request.CacheKey != null)
        {
            await cache.RemoveAsync(request.CacheKey, cancellationToken);
        }

        return response;
    }
}
