namespace OrderGrid.Domain.Orders;

public enum OrderStatus
{
    Submitted,
    InventoryReserved,
    PaymentAuthorized,
    ReadyForFulfillment,
    Shipped,
    Delivered,
    Cancelled,
    Failed
}
