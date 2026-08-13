using OrderGrid.Domain.Common;
using OrderGrid.Domain.Inventory;
namespace OrderGrid.Domain.Tests;
public sealed class InventoryItemTests
{
    [Fact] public void Reserve_and_release_preserve_balance()
    { var now = DateTimeOffset.UtcNow; var item = new InventoryItem("demo", "az-100", "Book", 10, now);
      item.Reserve(4, now); item.Release(2, now); Assert.Equal(8, item.AvailableQuantity); Assert.Equal(2, item.ReservedQuantity); }
    [Fact] public void Rejects_insufficient_stock()
    { var item = new InventoryItem("demo", "AZ-100", "Book", 2, DateTimeOffset.UtcNow);
      Assert.Throws<DomainException>(() => item.Reserve(3, DateTimeOffset.UtcNow)); }
}
