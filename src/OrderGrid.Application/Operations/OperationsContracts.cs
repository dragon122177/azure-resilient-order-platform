namespace OrderGrid.Application.Operations;
public sealed record AuditResponse(Guid Id, string Actor, string Action, string ResourceType,
    string ResourceId, string CorrelationId, DateTimeOffset OccurredAt, string? Details);
