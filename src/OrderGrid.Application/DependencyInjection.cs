using Microsoft.Extensions.DependencyInjection;
using OrderGrid.Application.Inventory;
using OrderGrid.Application.Operations;
using OrderGrid.Application.Orders;
using OrderGrid.Application.Workflows;

namespace OrderGrid.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderWorkflow, OrderWorkflow>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IOperationsService, OperationsService>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        return services;
    }
}
