using OrderGrid.Application.Abstractions;
namespace OrderGrid.Application.Inventory;
public sealed class InventoryService(IInventoryRepository inventory, IRequestContext context) : IInventoryService
{
    public async Task<IReadOnlyList<InventoryResponse>> ListAsync(CancellationToken token) =>
        (await inventory.ListAsync(context.TenantId, token))
        .Select(item => new InventoryResponse(item.Sku, item.Name, item.AvailableQuantity,
            item.ReservedQuantity, item.UpdatedAt)).ToArray();
}
