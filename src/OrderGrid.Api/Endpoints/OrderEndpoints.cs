using OrderGrid.Api.Security;
using OrderGrid.Application.Operations;
using OrderGrid.Application.Orders;
using OrderGrid.Domain.Orders;
namespace OrderGrid.Api.Endpoints;
public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/orders").WithTags("Orders");
        group.MapGet("/", async (OrderStatus? status, int page, int pageSize,
            IOrderService service, CancellationToken token) =>
            Results.Ok(await service.ListAsync(status, page, pageSize, token)))
            .RequireAuthorization(AuthorizationPolicies.ReadOrders).WithName("ListOrders");
        group.MapGet("/{orderId:guid}", async (Guid orderId, IOrderService service,
            CancellationToken token) => Results.Ok(await service.GetAsync(orderId, token)))
            .RequireAuthorization(AuthorizationPolicies.ReadOrders).WithName("GetOrder");
        group.MapPost("/", async (HttpContext http, CreateOrderCommand command,
            IOrderService service, IIdempotencyService idempotency, CancellationToken token) =>
        {
            var key = http.Request.Headers["Idempotency-Key"].FirstOrDefault() ?? string.Empty;
            var result = await idempotency.ExecuteAsync(key, command,
                inner => service.CreateAsync(command, inner), token);
            http.Response.Headers["Idempotency-Replayed"] = result.Replayed.ToString().ToLowerInvariant();
            return Results.Created($"/api/v1/orders/{result.Value.Id}", result.Value);
        }).RequireAuthorization(AuthorizationPolicies.WriteOrders).WithName("CreateOrder");
        group.MapPost("/{orderId:guid}/cancel", async (Guid orderId, CancelOrderCommand command,
            IOrderService service, CancellationToken token) =>
            Results.Ok(await service.CancelAsync(orderId, command, token)))
            .RequireAuthorization(AuthorizationPolicies.WriteOrders).WithName("CancelOrder");
        group.MapPost("/{orderId:guid}/ship", async (Guid orderId, ShipOrderCommand command,
            IOrderService service, CancellationToken token) =>
            Results.Ok(await service.ShipAsync(orderId, command, token)))
            .RequireAuthorization(AuthorizationPolicies.WriteOrders).WithName("ShipOrder");
        group.MapPost("/{orderId:guid}/deliver", async (Guid orderId, IOrderService service,
            CancellationToken token) => Results.Ok(await service.DeliverAsync(orderId, token)))
            .RequireAuthorization(AuthorizationPolicies.WriteOrders).WithName("DeliverOrder");
        return endpoints;
    }
}
