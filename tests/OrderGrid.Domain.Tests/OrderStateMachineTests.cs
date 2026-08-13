using OrderGrid.Domain.Common;
using OrderGrid.Domain.Orders;
using OrderGrid.Domain.ValueObjects;
namespace OrderGrid.Domain.Tests;
public sealed class OrderStateMachineTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);
    [Fact] public void Create_calculates_total_and_raises_event()
    { var order = Create(); Assert.Equal(5_000m, order.TotalAmount); Assert.IsType<OrderSubmitted>(order.DomainEvents.Single()); }
    [Fact] public void Happy_path_reaches_delivered()
    { var order = Create(); order.ReserveInventory(Now); order.AuthorizePayment("pay", Now);
      order.MarkReadyForFulfillment(Now); order.MarkShipped("Yamato", "T-1", Now); order.MarkDelivered(Now);
      Assert.Equal(OrderStatus.Delivered, order.Status); }
    [Fact] public void Cannot_ship_early() => Assert.Throws<DomainException>(() => Create().MarkShipped("Y", "T", Now));
    [Fact] public void Failed_order_is_terminal()
    { var order = Create(); order.Fail("Rejected", Now); Assert.Throws<DomainException>(() => order.Cancel("No", Now)); }
    private static Order Create() => Order.Create("demo", "EXT-1", "customer@example.com",
        new ShippingAddress("Aiko", "1 Shibuya", "Tokyo", "150-0002", "JP"),
        [("AZ-100", "Book", 2, new Money(2_500m, "JPY"))], Now);
}
