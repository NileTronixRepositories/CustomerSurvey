using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlock.Application.Behaviors
{
    public sealed class QueryCacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly ICacheService _cache;
        private readonly KeyedSemaphore _locks;
        private readonly ILogger<QueryCacheBehavior<TRequest, TResponse>> _log;

        private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

        public QueryCacheBehavior(ICacheService cache, KeyedSemaphore locks, ILogger<QueryCacheBehavior<TRequest, TResponse>> log)
            => (_cache, _locks, _log) = (cache, locks, log);

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (request is not ICacheableQuery<TResponse> cacheable)
                return await next();

            var key = cacheable.CacheKey ?? CacheKeyFactory.BuildFromRequest(request!);
            var ttl = cacheable.Ttl ?? DefaultTtl;

            var tags = (cacheable.Tags ?? Enumerable.Empty<string>())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var (found, cached) = await _cache.TryGetAsync<TResponse>(key, ct);
            if (found)
            {
                _log.LogDebug("CACHE HIT key={Key}", key);
                return cached!;
            }

            using (await _locks.WaitAsync(key, ct))
            {
                // double-check
                (found, cached) = await _cache.TryGetAsync<TResponse>(key, ct);
                if (found)
                {
                    _log.LogDebug("CACHE HIT(after-lock) key={Key}", key);
                    return cached!;
                }

                _log.LogDebug("CACHE MISS key={Key}", key);
                var res = await next();

                // نفس سياستك: كاش بس لو success
                if (ResultInspector.IsSuccess(res))
                    await _cache.SetAsync(key, res, ttl, tags, ct);

                return res;
            }
        }
    }
}