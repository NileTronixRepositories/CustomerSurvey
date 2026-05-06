using BuildingBlock.Domain.Primitive;

namespace BuildingBlock.Domain.EntitiesHelper
{
    public class AggregateRoot<TKey> : Entity<TKey>, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        protected AggregateRoot(TKey id)
            : base(id) { }

        protected AggregateRoot()
        { }

        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}