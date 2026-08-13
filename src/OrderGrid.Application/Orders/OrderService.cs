using System.Text.RegularExpressions;
using OrderGrid.Application.Abstractions;
using OrderGrid.Application.Common;
using OrderGrid.Domain.Operations;
using OrderGrid.Domain.Orders;
using OrderGrid.Domain.ValueObjects;

namespace OrderGrid.Application.Orders;

public sealed class OrderService(IOrderRepository orders, IInventoryRepository inventory,
    IOperationsRepository operations, IUnitOfWork unitOfWork, IRequestContext context,
    IClock clock) : IOrderService
{
    private static readonly Regex EmailPattern = new("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    public async Task<OrderResponse> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        Validate(command);
        if (await orders.GetByExternalReferenceAsync(context.TenantId, command.ExternalReference, cancellationToken) is not null)
            throw new ConflictException($"External reference '{command.ExternalReference}' already exists for this tenant.");

        var address = new ShippingAddress(command.ShippingAddress.Recipient,
            command.ShippingAddress.Line1, command.ShippingAddress.City,
            command.ShippingAddress.PostalCode, command.ShippingAddress.CountryCode,
            command.ShippingAddress.Line2, command.ShippingAddress.Region);
        var items = command.Items.Select(item => (item.Sku, item.Name, item.Quantity,
            new Money(item.UnitPrice, command.Currency)));
        var order = Order.Create(context.TenantId, command.ExternalReference,
            command.CustomerEmail, address, items, clock.UtcNow);
        await orders.AddAsync(order, cancellationToken);
        operations.AddAudit(Audit("order.created", order.Id, $"status={order.Status}"));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.ToResponse();
    }

    public async Task<OrderResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        (await RequiredAsync(id, cancellationToken)).ToResponse();

    public async Task<PagedOrdersResponse> ListAsync(OrderStatus? status, int page,
        int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100);
        var data = await orders.ListAsync(context.TenantId, status, page, pageSize, cancellationToken);
        var count = await orders.CountAsync(context.TenantId, status, cancellationToken);
        return new(data.Select(OrderMapper.ToResponse).ToArray(), page, pageSize, count);
    }

    public async Task<OrderResponse> CancelAsync(Guid id, CancelOrderCommand command,
        CancellationToken cancellationToken)
    {
        Order? result = null;
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var order = await RequiredAsync(id, token);
            if (order.Status is OrderStatus.InventoryReserved or OrderStatus.PaymentAuthorized
                or OrderStatus.ReadyForFulfillment)
            {
                foreach (var line in order.Items)
                {
                    var stock = await inventory.GetBySkuAsync(order.TenantId, line.Sku, token)
                        ?? throw new ConflictException($"No inventory record exists for SKU {line.Sku}.");
                    stock.Release(line.Quantity, clock.UtcNow);
                }
            }
            order.Cancel(command.Reason, clock.UtcNow);
            operations.AddAudit(Audit("order.cancelled", order.Id, $"reason={command.Reason}"));
            await unitOfWork.SaveChangesAsync(token);
            result = order;
        }, cancellationToken);
        return result!.ToResponse();
    }

    public async Task<OrderResponse> ShipAsync(Guid id, ShipOrderCommand command,
        CancellationToken cancellationToken)
    {
        var order = await RequiredAsync(id, cancellationToken);
        order.MarkShipped(command.Carrier, command.TrackingNumber, clock.UtcNow);
        operations.AddAudit(Audit("order.shipped", order.Id, $"carrier={command.Carrier}"));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.ToResponse();
    }

    public async Task<OrderResponse> DeliverAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await RequiredAsync(id, cancellationToken);
        order.MarkDelivered(clock.UtcNow);
        operations.AddAudit(Audit("order.delivered", order.Id));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.ToResponse();
    }

    public async Task<OrderMetricsResponse> GetMetricsAsync(CancellationToken cancellationToken)
    {
        var snapshot = await orders.GetMetricsAsync(context.TenantId, cancellationToken);
        int Count(OrderStatus status) => snapshot.ByStatus.GetValueOrDefault(status);
        return new(snapshot.TotalOrders,
            snapshot.TotalOrders - Count(OrderStatus.Delivered) - Count(OrderStatus.Cancelled) - Count(OrderStatus.Failed),
            Count(OrderStatus.Delivered), Count(OrderStatus.Failed), snapshot.GrossValue,
            snapshot.ByStatus.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value));
    }

    private async Task<Order> RequiredAsync(Guid id, CancellationToken token) =>
        await orders.GetAsync(context.TenantId, id, token)
        ?? throw new ResourceNotFoundException($"Order '{id}' was not found.");

    private AuditEntry Audit(string action, Guid id, string? details = null) =>
        new(context.TenantId, context.Actor, action, "order", id.ToString(),
            context.CorrelationId, clock.UtcNow, details);

    private static void Validate(CreateOrderCommand command)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(command.ExternalReference))
            errors[nameof(command.ExternalReference)] = ["External reference is required."];
        if (string.IsNullOrWhiteSpace(command.CustomerEmail) || !EmailPattern.IsMatch(command.CustomerEmail))
            errors[nameof(command.CustomerEmail)] = ["A valid customer email is required."];
        if (string.IsNullOrWhiteSpace(command.Currency) || command.Currency.Trim().Length != 3)
            errors[nameof(command.Currency)] = ["Currency must be a three-letter ISO code."];
        if (command.ShippingAddress is null)
            errors[nameof(command.ShippingAddress)] = ["Shipping address is required."];
        if (command.Items is null || command.Items.Count is < 1 or > 100)
            errors[nameof(command.Items)] = ["Provide between 1 and 100 order items."];
        if (errors.Count > 0) throw new RequestValidationException(errors);
    }
}
