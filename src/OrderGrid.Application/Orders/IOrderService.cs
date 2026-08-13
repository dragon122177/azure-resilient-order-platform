using OrderGrid.Domain.Orders;
namespace OrderGrid.Application.Orders;
public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);
    Task<OrderResponse> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedOrdersResponse> ListAsync(OrderStatus? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<OrderResponse> CancelAsync(Guid id, CancelOrderCommand command, CancellationToken cancellationToken);
    Task<OrderResponse> ShipAsync(Guid id, ShipOrderCommand command, CancellationToken cancellationToken);
    Task<OrderResponse> DeliverAsync(Guid id, CancellationToken cancellationToken);
    Task<OrderMetricsResponse> GetMetricsAsync(CancellationToken cancellationToken);
}
