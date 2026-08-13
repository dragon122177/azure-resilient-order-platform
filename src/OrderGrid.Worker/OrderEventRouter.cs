using OrderGrid.Application.Messaging;
using OrderGrid.Application.Workflows;
namespace OrderGrid.Worker;
public sealed class OrderEventRouter(IOrderWorkflow workflow) : IEventRouter
{
    public Task RouteAsync(EventEnvelope envelope, CancellationToken token)
    {
        if (!envelope.Data.TryGetProperty("orderId", out var element)
            || !Guid.TryParse(element.GetString(), out var orderId))
            throw new InvalidOperationException($"Event {envelope.Id} has no valid orderId.");
        return envelope.Type switch
        {
            "OrderSubmitted" => workflow.ReserveInventoryAsync(orderId, token),
            "InventoryReserved" => workflow.AuthorizePaymentAsync(orderId, token),
            "PaymentAuthorized" => workflow.PrepareFulfillmentAsync(orderId, token),
            "OrderReadyForFulfillment" or "OrderShipped" or "OrderDelivered"
                or "OrderCancelled" or "OrderFailed" => Task.CompletedTask,
            _ => throw new InvalidOperationException($"Unsupported event type '{envelope.Type}'.")
        };
    }
}
