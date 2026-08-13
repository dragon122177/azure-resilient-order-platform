using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OrderGrid.Application.Abstractions;
using OrderGrid.Application.Messaging;
using OrderGrid.Domain.Common;
using OrderGrid.Domain.Operations;
using OrderGrid.Infrastructure;
using OrderGrid.Infrastructure.Context;
namespace OrderGrid.Worker;
public sealed class ServiceBusConsumerWorker(IServiceProvider root, IServiceScopeFactory scopes,
    InfrastructureOptions options, ILogger<ServiceBusConsumerWorker> logger) : BackgroundService
{
    private ServiceBusSessionProcessor? _processor;
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        if (!options.MessagingMode.Equals("ServiceBus", StringComparison.OrdinalIgnoreCase)) return;
        _processor = root.GetRequiredService<ServiceBusClient>().CreateSessionProcessor(
            options.ServiceBusTopic, options.ServiceBusSubscription,
            new ServiceBusSessionProcessorOptions { AutoCompleteMessages = false,
                MaxConcurrentSessions = 8, MaxConcurrentCallsPerSession = 1,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5), PrefetchCount = 20 });
        _processor.ProcessMessageAsync += ProcessAsync;
        _processor.ProcessErrorAsync += ErrorAsync;
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
        await using var scope = scopes.CreateAsyncScope();
        var envelope = JsonSerializer.Deserialize<EventEnvelope>(args.Message.Body)
            ?? throw new InvalidOperationException("Invalid event envelope.");
        var operations = scope.ServiceProvider.GetRequiredService<IOperationsRepository>();
        var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var context = scope.ServiceProvider.GetRequiredService<RequestContext>();
        var router = scope.ServiceProvider.GetRequiredService<IEventRouter>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        const string consumer = "order-orchestrator";
        context.Set(envelope.TenantId, consumer, envelope.CorrelationId);
        if (await operations.HasProcessedAsync(consumer, args.Message.MessageId, args.CancellationToken))
        { await args.CompleteMessageAsync(args.Message, args.CancellationToken); return; }
        try
        {
            await unit.ExecuteInTransactionAsync(async token =>
            {
                await router.RouteAsync(envelope, token);
                operations.MarkProcessed(new InboxMessage(consumer, args.Message.MessageId, clock.UtcNow));
                await unit.SaveChangesAsync(token);
            }, args.CancellationToken);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (DomainException exception)
        {
            await args.DeadLetterMessageAsync(args.Message, "BusinessRuleRejected",
                exception.Message, args.CancellationToken);
            logger.LogWarning(exception, "Dead-lettered event {EventId}", envelope.Id);
        }
    }

    private Task ErrorAsync(ProcessErrorEventArgs args)
    { logger.LogError(args.Exception, "Service Bus error {Source} {Path}", args.ErrorSource, args.EntityPath); return Task.CompletedTask; }
}
