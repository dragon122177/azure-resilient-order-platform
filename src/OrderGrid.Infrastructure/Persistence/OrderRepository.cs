using Microsoft.EntityFrameworkCore;
using OrderGrid.Application.Abstractions;
using OrderGrid.Domain.Orders;

namespace OrderGrid.Infrastructure.Persistence;
public sealed class OrderRepository(OrderGridDbContext db) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken token) => await db.Orders.AddAsync(order, token);
    public Task<Order?> GetAsync(string tenant, Guid id, CancellationToken token) => db.Orders
        .Include(x => x.Items).SingleOrDefaultAsync(x => x.TenantId == tenant && x.Id == id, token);
    public Task<Order?> GetByExternalReferenceAsync(string tenant, string reference, CancellationToken token) =>
        db.Orders.Include(x => x.Items).SingleOrDefaultAsync(
            x => x.TenantId == tenant && x.ExternalReference == reference, token);

    public async Task<IReadOnlyList<Order>> ListAsync(string tenant, OrderStatus? status,
        int page, int pageSize, CancellationToken token)
    {
        var query = db.Orders.AsNoTracking().Include(x => x.Items).Where(x => x.TenantId == tenant);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize)
            .Take(pageSize).ToListAsync(token);
    }

    public Task<int> CountAsync(string tenant, OrderStatus? status, CancellationToken token)
    {
        var query = db.Orders.Where(x => x.TenantId == tenant);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return query.CountAsync(token);
    }

    public async Task<OrderMetricsSnapshot> GetMetricsAsync(string tenant, CancellationToken token)
    {
        var query = db.Orders.AsNoTracking().Where(x => x.TenantId == tenant);
        var groups = await query.GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync(token);
        var total = groups.Sum(x => x.Count);
        var value = total == 0 ? 0 : await query.SumAsync(x => x.TotalAmount, token);
        return new(total, value, groups.ToDictionary(x => x.Status, x => x.Count));
    }
}
