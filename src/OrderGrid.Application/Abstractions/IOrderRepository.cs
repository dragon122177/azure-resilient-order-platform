using OrderGrid.Domain.Orders;
namespace OrderGrid.Application.Abstractions;

public sealed record OrderMetricsSnapshot(int TotalOrders, decimal GrossValue,
    IReadOnlyDictionary<OrderStatus, int> ByStatus);

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetAsync(string tenantId, Guid id, CancellationToken cancellationToken);
    Task<Order?> GetByExternalReferenceAsync(string tenantId, string reference, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> ListAsync(string tenantId, OrderStatus? status, int page, int pageSize,
        CancellationToken cancellationToken);
    Task<int> CountAsync(string tenantId, OrderStatus? status, CancellationToken cancellationToken);
    Task<OrderMetricsSnapshot> GetMetricsAsync(string tenantId, CancellationToken cancellationToken);
}
