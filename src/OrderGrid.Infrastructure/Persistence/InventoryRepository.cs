using Microsoft.EntityFrameworkCore;
using OrderGrid.Application.Abstractions;
using OrderGrid.Domain.Inventory;
namespace OrderGrid.Infrastructure.Persistence;
public sealed class InventoryRepository(OrderGridDbContext db) : IInventoryRepository
{
    public Task<InventoryItem?> GetBySkuAsync(string tenant, string sku, CancellationToken token) =>
        db.Inventory.SingleOrDefaultAsync(x => x.TenantId == tenant && x.Sku == sku.ToUpper(), token);
    public async Task<IReadOnlyList<InventoryItem>> ListAsync(string tenant, CancellationToken token) =>
        await db.Inventory.AsNoTracking().Where(x => x.TenantId == tenant)
            .OrderBy(x => x.Sku).ToListAsync(token);
}
