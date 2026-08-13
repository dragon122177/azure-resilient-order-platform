using OrderGrid.Application.Abstractions;
using OrderGrid.Application.Messaging;
using OrderGrid.Infrastructure;
using OrderGrid.Infrastructure.Context;
namespace OrderGrid.Worker;
public sealed class OutboxDispatcherWorker(IServiceScopeFactory scopeFactory,
    InfrastructureOptions options, ILogger<OutboxDispatcherWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(750));
        do
        {
            try { await DispatchAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { logger.LogError(exception, "Outbox dispatch cycle failed"); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DispatchAsync(CancellationToken token)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var operations = scope.ServiceProvider.GetRequiredService<IOperationsRepository>();
        var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var context = scope.ServiceProvider.GetRequiredService<RequestContext>();
        var router = scope.ServiceProvider.GetRequiredService<IEventRouter>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        foreach (var message in await operations.GetPendingOutboxAsync(25, token))
        {
            context.Set(message.TenantId, "outbox-dispatcher", message.CorrelationId);
            var envelope = EventEnvelope.FromJson(message.Id, message.EventType, message.TenantId,
                message.CorrelationId, message.OccurredAt, message.Payload);
            try
            {
                if (options.MessagingMode.Equals("Local", StringComparison.OrdinalIgnoreCase))
                {
                    await unit.ExecuteInTransactionAsync(async inner =>
                    {
                        await router.RouteAsync(envelope, inner);
                        message.MarkPublished(clock.UtcNow);
                        await unit.SaveChangesAsync(inner);
                    }, token);
                }
                else
                {
                    await publisher.PublishAsync(envelope, token);
                    message.MarkPublished(clock.UtcNow);
                    await unit.SaveChangesAsync(token);
                }
                logger.LogInformation("Dispatched {EventType} {EventId}", message.EventType, message.Id);
            }
            catch (Exception exception)
            {
                message.MarkFailed(exception.Message); await unit.SaveChangesAsync(token);
                logger.LogWarning(exception, "Dispatch failed {EventId}; attempt {Attempt}",
                    message.Id, message.AttemptCount);
            }
        }
    }
}
