namespace OrderGrid.Application.Inventory;
public interface IInventoryService
{ Task<IReadOnlyList<InventoryResponse>> ListAsync(CancellationToken cancellationToken); }
