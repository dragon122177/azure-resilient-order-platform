using Microsoft.Extensions.Logging;
using OrderGrid.Application.Abstractions;
using OrderGrid.Application.Messaging;
namespace OrderGrid.Infrastructure.Messaging;
public sealed class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync(EventEnvelope envelope, CancellationToken token)
    {
        logger.LogInformation("Published local event {Type} {Id}", envelope.Type, envelope.Id);
        return Task.CompletedTask;
    }
}
