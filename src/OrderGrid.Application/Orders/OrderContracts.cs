using OrderGrid.Domain.Orders;
namespace OrderGrid.Application.Orders;

public sealed record CreateOrderItem(string Sku, string Name, int Quantity, decimal UnitPrice);
public sealed record CreateShippingAddress(string Recipient, string Line1, string City,
    string PostalCode, string CountryCode, string? Line2 = null, string? Region = null);
public sealed record CreateOrderCommand(string ExternalReference, string CustomerEmail,
    string Currency, CreateShippingAddress ShippingAddress, IReadOnlyList<CreateOrderItem> Items);
public sealed record CancelOrderCommand(string Reason);
public sealed record ShipOrderCommand(string Carrier, string TrackingNumber);
public sealed record OrderItemResponse(string Sku, string Name, int Quantity,
    decimal UnitPrice, decimal LineTotal);
public sealed record OrderResponse(Guid Id, string ExternalReference, string CustomerEmail,
    OrderStatus Status, decimal Total, string Currency, IReadOnlyList<OrderItemResponse> Items,
    string CountryCode, string? PaymentReference, string? Carrier, string? TrackingNumber,
    string? FailureReason, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt);
public sealed record PagedOrdersResponse(IReadOnlyList<OrderResponse> Items, int Page,
    int PageSize, int TotalCount);
public sealed record OrderMetricsResponse(int TotalOrders, int ActiveOrders, int CompletedOrders,
    int FailedOrders, decimal GrossValue, IReadOnlyDictionary<string, int> ByStatus);
