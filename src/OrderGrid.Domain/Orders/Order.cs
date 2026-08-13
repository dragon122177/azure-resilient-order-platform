using OrderGrid.Domain.Common;
using OrderGrid.Domain.ValueObjects;

namespace OrderGrid.Domain.Orders;

public sealed class Order : AggregateRoot
{
    private Order() { }

    private Order(Guid id, string tenantId, string externalReference, string customerEmail,
        ShippingAddress address, DateTimeOffset now)
    {
        Id = id;
        TenantId = Required(tenantId, nameof(tenantId), 64);
        ExternalReference = Required(externalReference, nameof(externalReference), 100);
        CustomerEmail = Required(customerEmail, nameof(customerEmail), 254).ToLowerInvariant();
        Recipient = address.Recipient;
        AddressLine1 = address.Line1;
        AddressLine2 = address.Line2;
        City = address.City;
        Region = address.Region;
        PostalCode = address.PostalCode;
        CountryCode = address.CountryCode;
        Status = OrderStatus.Submitted;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public string ExternalReference { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string Recipient { get; private set; } = string.Empty;
    public string AddressLine1 { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string? Region { get; private set; }
    public string PostalCode { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? PaymentReference { get; private set; }
    public string? Carrier { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public List<OrderItem> Items { get; private set; } = [];
    public Money Total => new(TotalAmount, Currency);
    public ShippingAddress ShippingAddress => new(Recipient, AddressLine1, City, PostalCode,
        CountryCode, AddressLine2, Region);

    public static Order Create(string tenantId, string externalReference, string customerEmail,
        ShippingAddress address,
        IEnumerable<(string Sku, string Name, int Quantity, Money UnitPrice)> items,
        DateTimeOffset now)
    {
        var order = new Order(Guid.NewGuid(), tenantId, externalReference, customerEmail, address, now);
        var requested = items.ToArray();
        if (requested.Length is < 1 or > 100)
            throw new DomainException("An order must contain between 1 and 100 items.");

        var total = Money.Zero(requested[0].UnitPrice.Currency);
        foreach (var item in requested)
        {
            var line = new OrderItem(order.Id, item.Sku, item.Name, item.Quantity, item.UnitPrice);
            total = total.Add(line.LineTotal);
            order.Items.Add(line);
        }
        if (total.Amount <= 0) throw new DomainException("Order total must be greater than zero.");
        order.TotalAmount = total.Amount;
        order.Currency = total.Currency;
        order.Raise(new OrderSubmitted(Guid.NewGuid(), now, order.Id, order.TenantId,
            total.Amount, total.Currency));
        return order;
    }

    public void ReserveInventory(DateTimeOffset now)
    {
        Ensure(OrderStatus.Submitted);
        Status = OrderStatus.InventoryReserved;
        Touch(now);
        Raise(new InventoryReserved(Guid.NewGuid(), now, Id, TenantId));
    }

    public void AuthorizePayment(string reference, DateTimeOffset now)
    {
        Ensure(OrderStatus.InventoryReserved);
        PaymentReference = Required(reference, nameof(reference), 120);
        Status = OrderStatus.PaymentAuthorized;
        Touch(now);
        Raise(new PaymentAuthorized(Guid.NewGuid(), now, Id, TenantId, PaymentReference));
    }

    public void MarkReadyForFulfillment(DateTimeOffset now)
    {
        Ensure(OrderStatus.PaymentAuthorized);
        Status = OrderStatus.ReadyForFulfillment;
        Touch(now);
        Raise(new OrderReadyForFulfillment(Guid.NewGuid(), now, Id, TenantId));
    }

    public void MarkShipped(string carrier, string trackingNumber, DateTimeOffset now)
    {
        Ensure(OrderStatus.ReadyForFulfillment);
        Carrier = Required(carrier, nameof(carrier), 80);
        TrackingNumber = Required(trackingNumber, nameof(trackingNumber), 120);
        Status = OrderStatus.Shipped;
        Touch(now);
        Raise(new OrderShipped(Guid.NewGuid(), now, Id, TenantId, Carrier, TrackingNumber));
    }

    public void MarkDelivered(DateTimeOffset now)
    {
        Ensure(OrderStatus.Shipped);
        Status = OrderStatus.Delivered;
        CompletedAt = now;
        Touch(now);
        Raise(new OrderDelivered(Guid.NewGuid(), now, Id, TenantId));
    }

    public void Cancel(string reason, DateTimeOffset now)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Failed)
            throw new DomainException($"Order in status {Status} cannot be cancelled.");
        FailureReason = Required(reason, nameof(reason), 500);
        Status = OrderStatus.Cancelled;
        CompletedAt = now;
        Touch(now);
        Raise(new OrderCancelled(Guid.NewGuid(), now, Id, TenantId, FailureReason));
    }

    public void Fail(string reason, DateTimeOffset now)
    {
        if (Status is not (OrderStatus.Submitted or OrderStatus.InventoryReserved or OrderStatus.PaymentAuthorized))
            throw new DomainException($"Order in status {Status} cannot be failed.");
        FailureReason = Required(reason, nameof(reason), 500);
        Status = OrderStatus.Failed;
        CompletedAt = now;
        Touch(now);
        Raise(new OrderFailed(Guid.NewGuid(), now, Id, TenantId, FailureReason));
    }

    private void Ensure(OrderStatus required)
    {
        if (Status != required)
            throw new DomainException($"Expected order status {required}, but current status is {Status}.");
    }

    private void Touch(DateTimeOffset now) => UpdatedAt = now;

    private static string Required(string value, string name, int max)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length is 0 || normalized.Length > max)
            throw new DomainException($"{name} is required and must be at most {max} characters.");
        return normalized;
    }
}
