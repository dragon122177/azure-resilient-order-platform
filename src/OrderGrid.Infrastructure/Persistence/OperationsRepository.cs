using Microsoft.EntityFrameworkCore;
using OrderGrid.Application.Abstractions;
using OrderGrid.Domain.Operations;
namespace OrderGrid.Infrastructure.Persistence;
public sealed class OperationsRepository(OrderGridDbContext db) : IOperationsRepository
{
    public void AddAudit(AuditEntry entry) => db.AuditEntries.Add(entry);
    public async Task<IReadOnlyList<AuditEntry>> ListAuditAsync(string tenant, int limit, CancellationToken token) =>
        await db.AuditEntries.AsNoTracking().Where(x => x.TenantId == tenant)
            .OrderByDescending(x => x.OccurredAt).Take(limit).ToListAsync(token);
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingOutboxAsync(int batchSize, CancellationToken token) =>
        await db.OutboxMessages.Where(x => x.PublishedAt == null && x.AttemptCount < 10)
            .OrderBy(x => x.OccurredAt).Take(batchSize).ToListAsync(token);
    public Task<bool> HasProcessedAsync(string consumer, string id, CancellationToken token) =>
        db.InboxMessages.AnyAsync(x => x.Consumer == consumer && x.MessageId == id, token);
    public void MarkProcessed(InboxMessage message) => db.InboxMessages.Add(message);
    public Task<IdempotencyRecord?> GetIdempotencyAsync(string tenant, string key, CancellationToken token) =>
        db.IdempotencyRecords.SingleOrDefaultAsync(x => x.TenantId == tenant && x.Key == key, token);
    public void AddIdempotency(IdempotencyRecord record) => db.IdempotencyRecords.Add(record);
}
