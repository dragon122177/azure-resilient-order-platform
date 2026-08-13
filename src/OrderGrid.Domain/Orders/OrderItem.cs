using OrderGrid.Domain.Common;
using OrderGrid.Domain.ValueObjects;

namespace OrderGrid.Domain.Orders;

public sealed class OrderItem
{
    private OrderItem() { }

    internal OrderItem(Guid orderId, string sku, string name, int quantity, Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 64)
            throw new DomainException("SKU is required and must be at most 64 characters.");
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 160)
            throw new DomainException("Item name is required and must be at most 160 characters.");
        if (quantity is < 1 or > 1_000)
            throw new DomainException("Quantity must be between 1 and 1,000.");

        Id = Guid.NewGuid();
        OrderId = orderId;
        Sku = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        Quantity = quantity;
        UnitPriceAmount = unitPrice.Amount;
        Currency = unitPrice.Currency;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPriceAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public Money UnitPrice => new(UnitPriceAmount, Currency);
    public Money LineTotal => UnitPrice.Multiply(Quantity);
}
