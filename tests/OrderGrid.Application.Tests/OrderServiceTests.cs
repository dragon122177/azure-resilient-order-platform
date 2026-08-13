using Microsoft.EntityFrameworkCore;
using OrderGrid.Application.Common;
using OrderGrid.Application.Orders;
using OrderGrid.Domain.Inventory;
using OrderGrid.Domain.Orders;
namespace OrderGrid.Application.Tests;
public sealed class OrderServiceTests
{
    [Fact] public async Task Create_persists_order_audit_and_outbox()
    { await using var f = await ApplicationFixture.CreateAsync(); var response = await f.Orders.CreateAsync(Command(), default);
      Assert.Equal(OrderStatus.Submitted, response.Status); Assert.Single(await f.Db.Orders.ToListAsync());
      Assert.Single(await f.Db.AuditEntries.ToListAsync()); Assert.Single(await f.Db.OutboxMessages.ToListAsync()); }

    [Fact] public async Task Duplicate_external_reference_is_rejected()
    { await using var f = await ApplicationFixture.CreateAsync(); await f.Orders.CreateAsync(Command(), default);
      await Assert.ThrowsAsync<ConflictException>(() => f.Orders.CreateAsync(Command(), default)); }

    [Fact] public async Task Workflow_reaches_ready_for_fulfillment()
    { await using var f = await StockFixture(); var created = await f.Orders.CreateAsync(Command(), default);
      await f.Workflow.ReserveInventoryAsync(created.Id, default); await f.Workflow.AuthorizePaymentAsync(created.Id, default);
      await f.Workflow.PrepareFulfillmentAsync(created.Id, default); f.Db.ChangeTracker.Clear();
      Assert.Equal(OrderStatus.ReadyForFulfillment, (await f.Db.Orders.SingleAsync()).Status); }

    [Fact] public async Task Cancellation_releases_inventory()
    { await using var f = await StockFixture(); var created = await f.Orders.CreateAsync(Command(), default);
      await f.Workflow.ReserveInventoryAsync(created.Id, default); await f.Workflow.AuthorizePaymentAsync(created.Id, default);
      await f.Orders.CancelAsync(created.Id, new CancelOrderCommand("Customer request"), default); f.Db.ChangeTracker.Clear();
      var stock = await f.Db.Inventory.SingleAsync(); Assert.Equal(10, stock.AvailableQuantity); Assert.Equal(0, stock.ReservedQuantity); }

    [Fact] public async Task Payment_decline_compensates_inventory()
    { await using var f = await StockFixture(); var command = Command() with
      { ExternalReference = "DECLINE-1", CustomerEmail = "decline@example.com" };
      var created = await f.Orders.CreateAsync(command, default); await f.Workflow.ReserveInventoryAsync(created.Id, default);
      await f.Workflow.AuthorizePaymentAsync(created.Id, default); f.Db.ChangeTracker.Clear();
      Assert.Equal(OrderStatus.Failed, (await f.Db.Orders.SingleAsync()).Status);
      Assert.Equal(10, (await f.Db.Inventory.SingleAsync()).AvailableQuantity); }

    [Fact] public async Task Sqlite_orders_timestamped_queries()
    { await using var f = await ApplicationFixture.CreateAsync(); await f.Orders.CreateAsync(Command(), default);
      Assert.Single((await f.Orders.ListAsync(null, 1, 20, default)).Items);
      Assert.Single(await f.Operations.ListAuditAsync("demo", 20, default));
      Assert.Single(await f.Operations.GetPendingOutboxAsync(20, default)); }

    private static async Task<ApplicationFixture> StockFixture()
    { var f = await ApplicationFixture.CreateAsync(); f.Db.Inventory.Add(new InventoryItem("demo", "AZ-100", "Book", 10, f.Clock.UtcNow));
      await f.Db.SaveChangesAsync(); return f; }
    private static CreateOrderCommand Command() => new("WEB-001", "customer@example.com", "JPY",
        new CreateShippingAddress("Aiko", "1 Shibuya", "Tokyo", "150-0002", "JP"),
        [new CreateOrderItem("AZ-100", "Book", 2, 2_500m)]);
}
