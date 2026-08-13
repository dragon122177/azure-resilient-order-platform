using OrderGrid.Domain.Common;

namespace OrderGrid.Domain.Orders;

public sealed record OrderSubmitted(Guid EventId, DateTimeOffset OccurredAt, Guid OrderId,
    string TenantId, decimal Total, string Currency) : DomainEvent(EventId, OccurredAt)
{ public override string EventType => nameof(OrderSubmitted); }

public sealed record InventoryReserved(Guid EventId, DateTimeOffset OccurredAt, Guid OrderId,
    string TenantId) : DomainEvent(EventId, OccurredAt)
{ public override string EventType => nameof(InventoryReserved); }

public sealed record PaymentAuthorized(Guid EventId, DateTimeOffset OccurredAt, Guid OrderId,
    string TenantId, string PaymentReference) : DomainEvent(EventId, OccurredAt)
{ public override string EventType => nameof(PaymentAuthorized); }

public sealed record OrderReadyForFulfillment(Guid EventId, DateTimeOffset OccurredAt, Guid OrderId,
    string TenantId) : DomainEvent(EventId, OccurredAt)
{ public override string EventType => nameof(OrderReadyForFulfillment); }

public sealed record OrderShipped(Guid EventId, DateTimeOffset OccurredAt, Guid OrderId,
    string TenantId, string Carrier, string TrackingNumber) : DomainEvent(EventId, OccurredAt)
{ public override string EventType => nameof(OrderShipped); }

public sealed record OrderDelivered(Guid EventId, DateTimeOffset OccurredAt, Guid OrderId,
    string TenantId) : DomainEvent(EventId, OccurredAt)
{ public override string EventType => nameof(OrderDelivered); }

public sealed record OrderCancelled(Guid EventId, DateTimeOffset OccurredAt, Guid OrderId,
    string TenantId, string Reason) : DomainEvent(EventId, OccurredAt)
{ public override string EventType => nameof(OrderCancelled); }

public sealed record OrderFailed(Guid EventId, DateTimeOffset OccurredAt, Guid OrderId,
    string TenantId, string Reason) : DomainEvent(EventId, OccurredAt)
{ public override string EventType => nameof(OrderFailed); }
