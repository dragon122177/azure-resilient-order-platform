namespace OrderGrid.Application.Workflows;
public interface IOrderWorkflow
{
    Task ReserveInventoryAsync(Guid orderId, CancellationToken cancellationToken);
    Task AuthorizePaymentAsync(Guid orderId, CancellationToken cancellationToken);
    Task PrepareFulfillmentAsync(Guid orderId, CancellationToken cancellationToken);
}
