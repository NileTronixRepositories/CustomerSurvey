using BuildingBlock.Domain.Primitive;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BuildingBlock.Infrastracture.Interceptors;

public sealed class DomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;
    private readonly ILogger<DomainEventsInterceptor> _logger;

    // Guard per interceptor instance
    private bool _isDispatching;

    public DomainEventsInterceptor(IPublisher publisher, ILogger<DomainEventsInterceptor> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return result;

        // لو حابب تربطها بوجود rows affected سيب الشرط، لو لا شيله
        if (result <= 0)
            return result;

        if (_isDispatching)
            return result;

        try
        {
            _isDispatching = true;
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }
        finally
        {
            _isDispatching = false;
        }

        return result;
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken ct)
    {
        var entitiesWithEvents = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        if (entitiesWithEvents.Count == 0)
            return;

        var events = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToArray();

        foreach (var entity in entitiesWithEvents)
            entity.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            _logger.LogDebug("Publishing domain event {EventType}", domainEvent.GetType().Name);

            // لو دي Domain Events داخلية: خلّيها تفشل لو handler فشل
            await _publisher.Publish(domainEvent, ct);
        }
    }
}