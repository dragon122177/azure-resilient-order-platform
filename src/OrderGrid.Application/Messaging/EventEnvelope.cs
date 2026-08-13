using System.Text.Json;
using OrderGrid.Domain.Common;

namespace OrderGrid.Application.Messaging;

public sealed record EventEnvelope(Guid Id, string Type, int SchemaVersion, string TenantId,
    string CorrelationId, DateTimeOffset OccurredAt, JsonElement Data)
{
    public static EventEnvelope FromDomainEvent(IDomainEvent domainEvent, string tenantId,
        string correlationId)
    {
        var json = JsonSerializer.SerializeToElement(domainEvent, domainEvent.GetType());
        return new(domainEvent.EventId, domainEvent.EventType, 1, tenantId, correlationId,
            domainEvent.OccurredAt, json);
    }

    public static EventEnvelope FromJson(Guid id, string type, string tenantId,
        string correlationId, DateTimeOffset occurredAt, string payload) =>
        new(id, type, 1, tenantId, correlationId, occurredAt,
            JsonSerializer.Deserialize<JsonElement>(payload));
}
