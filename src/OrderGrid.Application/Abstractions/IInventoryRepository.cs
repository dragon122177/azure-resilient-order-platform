using OrderGrid.Domain.Inventory;
namespace OrderGrid.Application.Abstractions;
public interface IInventoryRepository
{
    Task<InventoryItem?> GetBySkuAsync(string tenantId, string sku, CancellationToken cancellationToken);
    Task<IReadOnlyList<InventoryItem>> ListAsync(string tenantId, CancellationToken cancellationToken);
}
