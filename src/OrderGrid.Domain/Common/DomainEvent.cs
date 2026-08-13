namespace OrderGrid.Domain.Common;

public abstract record DomainEvent(Guid EventId, DateTimeOffset OccurredAt) : IDomainEvent
{
    public abstract string EventType { get; }
}
