using OrderGrid.Domain.Orders;
namespace OrderGrid.Application.Orders;
public static class OrderMapper
{
    public static OrderResponse ToResponse(this Order order) => new(
        order.Id, order.ExternalReference, order.CustomerEmail, order.Status,
        order.TotalAmount, order.Currency,
        order.Items.Select(item => new OrderItemResponse(item.Sku, item.Name, item.Quantity,
            item.UnitPriceAmount, item.LineTotal.Amount)).ToArray(),
        order.CountryCode, order.PaymentReference, order.Carrier, order.TrackingNumber,
        order.FailureReason, order.CreatedAt, order.UpdatedAt, order.CompletedAt);
}
