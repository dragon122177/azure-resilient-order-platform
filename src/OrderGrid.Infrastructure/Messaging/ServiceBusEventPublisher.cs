using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OrderGrid.Application.Abstractions;
using OrderGrid.Application.Messaging;
namespace OrderGrid.Infrastructure.Messaging;
public sealed class ServiceBusEventPublisher(ServiceBusClient client, InfrastructureOptions options)
    : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender = client.CreateSender(options.ServiceBusTopic);
    public async Task PublishAsync(EventEnvelope envelope, CancellationToken token)
    {
        var message = new ServiceBusMessage(JsonSerializer.Serialize(envelope))
        {
            MessageId = envelope.Id.ToString(), CorrelationId = envelope.CorrelationId,
            Subject = envelope.Type, ContentType = "application/json",
            SessionId = envelope.Data.TryGetProperty("orderId", out var id) ? id.GetString() : envelope.TenantId
        };
        message.ApplicationProperties["tenantId"] = envelope.TenantId;
        message.ApplicationProperties["schemaVersion"] = envelope.SchemaVersion;
        await _sender.SendMessageAsync(message, token);
    }
    public async ValueTask DisposeAsync() => await _sender.DisposeAsync();
}
