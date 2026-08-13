using OrderGrid.Domain.Operations;
namespace OrderGrid.Application.Abstractions;
public interface IOperationsRepository
{
    void AddAudit(AuditEntry entry);
    Task<IReadOnlyList<AuditEntry>> ListAuditAsync(string tenantId, int limit, CancellationToken cancellationToken);
    Task<IReadOnlyList<OutboxMessage>> GetPendingOutboxAsync(int batchSize, CancellationToken cancellationToken);
    Task<bool> HasProcessedAsync(string consumer, string messageId, CancellationToken cancellationToken);
    void MarkProcessed(InboxMessage message);
    Task<IdempotencyRecord?> GetIdempotencyAsync(string tenantId, string key, CancellationToken cancellationToken);
    void AddIdempotency(IdempotencyRecord record);
}
