using OrderGrid.Api.Security;
using OrderGrid.Application.Inventory;
using OrderGrid.Application.Operations;
using OrderGrid.Application.Orders;
namespace OrderGrid.Api.Endpoints;
public static class OperationsEndpoints
{
    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/operations").WithTags("Operations")
            .RequireAuthorization(AuthorizationPolicies.ReadOperations);
        group.MapGet("/metrics", async (IOrderService service, CancellationToken token) =>
            Results.Ok(await service.GetMetricsAsync(token)));
        group.MapGet("/inventory", async (IInventoryService service, CancellationToken token) =>
            Results.Ok(await service.ListAsync(token)));
        group.MapGet("/audit", async (int limit, IOperationsService service, CancellationToken token) =>
            Results.Ok(await service.ListAuditAsync(limit, token)));
        return endpoints;
    }
}
