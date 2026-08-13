namespace OrderGrid.Application.Inventory;
public sealed record InventoryResponse(string Sku, string Name, int AvailableQuantity,
    int ReservedQuantity, DateTimeOffset UpdatedAt);
