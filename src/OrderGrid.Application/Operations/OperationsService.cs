using OrderGrid.Application.Abstractions;
namespace OrderGrid.Application.Operations;
public sealed class OperationsService(IOperationsRepository operations, IRequestContext context) : IOperationsService
{
    public async Task<IReadOnlyList<AuditResponse>> ListAuditAsync(int limit, CancellationToken token) =>
        (await operations.ListAuditAsync(context.TenantId, Math.Clamp(limit, 1, 100), token))
        .Select(entry => new AuditResponse(entry.Id, entry.Actor, entry.Action, entry.ResourceType,
            entry.ResourceId, entry.CorrelationId, entry.OccurredAt, entry.Details)).ToArray();
}
