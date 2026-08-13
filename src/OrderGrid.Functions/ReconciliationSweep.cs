using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderGrid.Infrastructure.Persistence;
namespace OrderGrid.Functions;
public sealed class ReconciliationSweep(OrderGridDbContext db, ILogger<ReconciliationSweep> logger)
{
    [Function(nameof(ReconciliationSweep))]
    public async Task RunAsync([TimerTrigger("0 */5 * * * *")] TimerInfo timer, CancellationToken token)
    {
        var threshold = DateTimeOffset.UtcNow.AddMinutes(-10);
        var delayed = await db.OutboxMessages.AsNoTracking().CountAsync(
            x => x.PublishedAt == null && x.OccurredAt < threshold, token);
        var exhausted = await db.OutboxMessages.AsNoTracking().CountAsync(
            x => x.PublishedAt == null && x.AttemptCount >= 10, token);
        logger.LogInformation("Reconciliation delayed={Delayed} exhausted={Exhausted} next={Next}",
            delayed, exhausted, timer.ScheduleStatus?.Next);
        if (exhausted > 0) logger.LogWarning("{Count} exhausted outbox messages require review", exhausted);
    }
}
