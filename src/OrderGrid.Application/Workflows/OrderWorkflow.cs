using OrderGrid.Application.Abstractions;
using OrderGrid.Application.Common;
using OrderGrid.Domain.Operations;

namespace OrderGrid.Application.Workflows;

public sealed class OrderWorkflow(IOrderRepository orders, IInventoryRepository inventory,
    IOperationsRepository operations, IUnitOfWork unitOfWork, IRequestContext context,
    IClock clock) : IOrderWorkflow
{
    public async Task ReserveInventoryAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var order = await RequiredAsync(orderId, token);
            foreach (var line in order.Items)
            {
                var stock = await inventory.GetBySkuAsync(order.TenantId, line.Sku, token)
                    ?? throw new ConflictException($"No inventory record exists for SKU {line.Sku}.");
                stock.Reserve(line.Quantity, clock.UtcNow);
            }
            order.ReserveInventory(clock.UtcNow);
            Audit(order.TenantId, orderId, "workflow.inventory_reserved");
            await unitOfWork.SaveChangesAsync(token);
        }, cancellationToken);
    }

    public async Task AuthorizePaymentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var order = await RequiredAsync(orderId, token);
            if (order.CustomerEmail.Contains("decline", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var line in order.Items)
                {
                    var stock = await inventory.GetBySkuAsync(order.TenantId, line.Sku, token)
                        ?? throw new ConflictException($"No inventory record exists for SKU {line.Sku}.");
                    stock.Release(line.Quantity, clock.UtcNow);
                }
                order.Fail("Payment simulator declined the order.", clock.UtcNow);
                Audit(order.TenantId, orderId, "workflow.payment_declined");
            }
            else
            {
                order.AuthorizePayment($"sim_{order.Id:N}", clock.UtcNow);
                Audit(order.TenantId, orderId, "workflow.payment_authorized");
            }
            await unitOfWork.SaveChangesAsync(token);
        }, cancellationToken);
    }

    public async Task PrepareFulfillmentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await RequiredAsync(orderId, cancellationToken);
        order.MarkReadyForFulfillment(clock.UtcNow);
        Audit(order.TenantId, orderId, "workflow.fulfillment_ready");
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Domain.Orders.Order> RequiredAsync(Guid orderId, CancellationToken token) =>
        await orders.GetAsync(context.TenantId, orderId, token)
        ?? throw new ResourceNotFoundException($"Order '{orderId}' was not found.");

    private void Audit(string tenantId, Guid id, string action) => operations.AddAudit(
        new AuditEntry(tenantId, context.Actor, action, "order", id.ToString(),
            context.CorrelationId, clock.UtcNow));
}
