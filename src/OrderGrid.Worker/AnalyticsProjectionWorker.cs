using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OrderGrid.Application.Abstractions;
using OrderGrid.Application.Messaging;
using OrderGrid.Infrastructure;
namespace OrderGrid.Worker;
public sealed class AnalyticsProjectionWorker(IServiceProvider root, IServiceScopeFactory scopes,
    InfrastructureOptions options, ILogger<AnalyticsProjectionWorker> logger) : BackgroundService
{
    private ServiceBusSessionProcessor? _processor;
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        if (!options.MessagingMode.Equals("ServiceBus", StringComparison.OrdinalIgnoreCase)) return;
        _processor = root.GetRequiredService<ServiceBusClient>().CreateSessionProcessor(
            options.ServiceBusTopic, options.ServiceBusAnalyticsSubscription,
            new ServiceBusSessionProcessorOptions { AutoCompleteMessages = false,
                MaxConcurrentSessions = 4, MaxConcurrentCallsPerSession = 1,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5), PrefetchCount = 20 });
        _processor.ProcessMessageAsync += ProcessAsync; _processor.ProcessErrorAsync += ErrorAsync;
        await _processor.StartProcessingAsync(token);
        try { await Task.Delay(Timeout.InfiniteTimeSpan, token); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }
    public override async Task StopAsync(CancellationToken token)
    {
        if (_processor is not null) { await _processor.StopProcessingAsync(token); await _processor.DisposeAsync(); }
        await base.StopAsync(token);
    }
    private async Task ProcessAsync(ProcessSessionMessageEventArgs args)
    {
        EventEnvelope envelope;
        try { envelope = JsonSerializer.Deserialize<EventEnvelope>(args.Message.Body)
                ?? throw new InvalidOperationException("Event envelope is empty."); }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            await args.DeadLetterMessageAsync(args.Message, "InvalidEnvelope", exception.Message, args.CancellationToken);
            return;
        }
        if (envelope.Type != "OrderDelivered")
        { await args.CompleteMessageAsync(args.Message, args.CancellationToken); return; }
        if (!envelope.Data.TryGetProperty("orderId", out var element)
            || !Guid.TryParse(element.GetString(), out var orderId))
        { await args.DeadLetterMessageAsync(args.Message, "InvalidOrderId", "Missing orderId", args.CancellationToken); return; }
        await using var scope = scopes.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IReceiptStore>();
        var projection = JsonSerializer.Serialize(new { envelope.Id, envelope.Type,
            envelope.TenantId, envelope.CorrelationId, envelope.OccurredAt, OrderId = orderId,
            ProjectedAt = DateTimeOffset.UtcNow });
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(projection));
        var uri = await store.StoreAsync(envelope.TenantId, orderId, stream, "application/json", args.CancellationToken);
        await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        logger.LogInformation("Projected delivered order {OrderId} to {Uri}", orderId, uri);
    }
    private Task ErrorAsync(ProcessErrorEventArgs args)
    { logger.LogError(args.Exception, "Analytics error {Source} {Path}", args.ErrorSource, args.EntityPath); return Task.CompletedTask; }
}
