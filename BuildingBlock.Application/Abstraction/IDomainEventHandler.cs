using BuildingBlock.Domain.Primitive;
using MediatR;

namespace BuildingBlock.Application.Abstraction
{
    public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
     where TEvent : IDomainEvent
    {
    }
}