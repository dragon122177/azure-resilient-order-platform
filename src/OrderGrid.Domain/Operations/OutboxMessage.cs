namespace OrderGrid.Domain.Operations;

public sealed class OutboxMessage
{
    private OutboxMessage() { }
    public OutboxMessage(Guid id, string tenantId, string eventType, string payload,
        DateTimeOffset occurredAt, string correlationId)
    {
        Id = id; TenantId = tenantId; EventType = eventType; Payload = payload;
        OccurredAt = occurredAt; CorrelationId = correlationId;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    public void MarkPublished(DateTimeOffset now) { PublishedAt = now; LastError = null; }
    public void MarkFailed(string error) { AttemptCount++; LastError = error.Length > 2_000 ? error[..2_000] : error; }
}
