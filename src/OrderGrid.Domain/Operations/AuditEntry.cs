namespace OrderGrid.Domain.Operations;

public sealed class AuditEntry
{
    private AuditEntry() { }
    public AuditEntry(string tenantId, string actor, string action, string resourceType,
        string resourceId, string correlationId, DateTimeOffset occurredAt, string? details = null)
    {
        Id = Guid.NewGuid(); TenantId = tenantId; Actor = actor; Action = action;
        ResourceType = resourceType; ResourceId = resourceId; CorrelationId = correlationId;
        OccurredAt = occurredAt; Details = details;
    }
    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public string Actor { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string ResourceType { get; private set; } = string.Empty;
    public string ResourceId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public string? Details { get; private set; }
}
