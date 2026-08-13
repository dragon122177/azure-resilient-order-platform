using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderGrid.Application.Abstractions;
using OrderGrid.Application.Messaging;
namespace OrderGrid.Functions;
public sealed class DeliveredOrderProjection(IReceiptStore store, ILogger<DeliveredOrderProjection> logger)
{
    [Function(nameof(DeliveredOrderProjection))]
    public async Task RunAsync([ServiceBusTrigger("%ServiceBusTopic%", "%ServiceBusFunctionsSubscription%",
        Connection = "ServiceBusConnection", IsSessionsEnabled = true)] string message, CancellationToken token)
    {
        var envelope = JsonSerializer.Deserialize<EventEnvelope>(message)
            ?? throw new InvalidOperationException("Invalid event envelope.");
        if (envelope.Type != "OrderDelivered") return;
        if (!envelope.Data.TryGetProperty("orderId", out var element)
            || !Guid.TryParse(element.GetString(), out var orderId))
            throw new InvalidOperationException("Delivered event is missing orderId.");
        var json = JsonSerializer.Serialize(new { envelope.Id, envelope.Type, envelope.TenantId,
            envelope.CorrelationId, envelope.OccurredAt, OrderId = orderId, ProjectedAt = DateTimeOffset.UtcNow });
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var uri = await store.StoreAsync(envelope.TenantId, orderId, stream, "application/json", token);
        logger.LogInformation("Stored delivered projection {OrderId} at {Uri}", orderId, uri);
    }
}
