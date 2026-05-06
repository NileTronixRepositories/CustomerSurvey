using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlock.Application.Behaviors
{
    public sealed class CommandCacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
    {
        private readonly ICacheService _cache;
        private readonly ILogger<CommandCacheInvalidationBehavior<TRequest, TResponse>> _log;

        public CommandCacheInvalidationBehavior(ICacheService cache, ILogger<CommandCacheInvalidationBehavior<TRequest, TResponse>> log)
            => (_cache, _log) = (cache, log);

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            var response = await next();

            if (request is ICommand && ResultInspector.IsSuccess(response))
            {
                if (request is ICacheInvalidator invalidator)
                {
                    var tags = (invalidator.Tags ?? Enumerable.Empty<string>())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    if (tags.Length > 0)
                    {
                        await _cache.InvalidateByTagsAsync(tags, ct);
                        _log.LogInformation("CACHE INVALIDATE tags=[{Tags}]", string.Join(",", tags));
                    }
                    else
                    {
                        _log.LogWarning("CACHE INVALIDATE skipped: ICacheInvalidator has NO tags.");
                    }
                }
                else
                {
                    _log.LogWarning("CACHE INVALIDATE skipped: ICommand does not implement ICacheInvalidator.");
                }
            }

            return response;
        }
    }
}