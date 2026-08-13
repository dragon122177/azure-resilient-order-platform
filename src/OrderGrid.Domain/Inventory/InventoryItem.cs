using OrderGrid.Domain.Common;

namespace OrderGrid.Domain.Inventory;

public sealed class InventoryItem
{
    private InventoryItem() { }

    public InventoryItem(string tenantId, string sku, string name, int availableQuantity, DateTimeOffset now)
    {
        if (availableQuantity < 0) throw new DomainException("Available inventory cannot be negative.");
        Id = Guid.NewGuid();
        TenantId = Required(tenantId, 64);
        Sku = Required(sku, 64).ToUpperInvariant();
        Name = Required(name, 160);
        AvailableQuantity = availableQuantity;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int AvailableQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public void Reserve(int quantity, DateTimeOffset now)
    {
        if (quantity < 1) throw new DomainException("Reservation quantity must be positive.");
        if (AvailableQuantity < quantity) throw new DomainException($"Insufficient inventory for SKU {Sku}.");
        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
        UpdatedAt = now;
    }

    public void Release(int quantity, DateTimeOffset now)
    {
        if (quantity < 1 || ReservedQuantity < quantity)
            throw new DomainException("Invalid inventory release quantity.");
        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;
        UpdatedAt = now;
    }

    private static string Required(string value, int max)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length is 0 || normalized.Length > max)
            throw new DomainException($"Value is required and must be at most {max} characters.");
        return normalized;
    }
}
