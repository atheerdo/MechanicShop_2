using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviors;

// This class is intercepts MediatR requests that support caching and serves results from cache if available.If not, it runs the real handler, caches the result, and returns it.
public class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(cancellationToken);
        }

        _logger.LogInformation("Checking cache for request {RequestType}", typeof(TRequest).Name);
        var result = await _cache.GetOrCreateAsync(
            key: cachedRequest.CacheKey,
            factory: async ct =>
        {
            var innerResult = await next(ct);

            if (innerResult is IResult r && r.IsSuccess)
            {
                return innerResult;
            }

            // Don't cache failures
            return default!;
        },
            options: new HybridCacheEntryOptions
            {
                Expiration = cachedRequest.Expiration
            },
            tags: cachedRequest.Tags,
            cancellationToken: cancellationToken);

        return result;
    }
}