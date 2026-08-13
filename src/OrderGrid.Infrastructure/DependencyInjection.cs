using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderGrid.Application.Abstractions;
using OrderGrid.Infrastructure.Context;
using OrderGrid.Infrastructure.Messaging;
using OrderGrid.Infrastructure.Persistence;
using OrderGrid.Infrastructure.Storage;
using OrderGrid.Infrastructure.Time;

namespace OrderGrid.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(InfrastructureOptions.SectionName)
            .Get<InfrastructureOptions>() ?? new();
        services.AddSingleton(options);
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<RequestContext>();
        services.AddScoped<IRequestContext>(provider => provider.GetRequiredService<RequestContext>());
        services.AddDbContext<OrderGridDbContext>(builder =>
        {
            if (options.DatabaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                builder.UseSqlServer(options.DatabaseConnectionString,
                    sql => { sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null); sql.CommandTimeout(30); });
            else builder.UseSqlite(options.DatabaseConnectionString);
        });
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IOperationsRepository, OperationsRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OrderGridDbContext>());
        services.AddScoped<DatabaseInitializer>();
        AddMessaging(services, options);
        AddStorage(services, options);
        services.AddHealthChecks().AddDbContextCheck<OrderGridDbContext>("database");
        return services;
    }

    private static void AddMessaging(IServiceCollection services, InfrastructureOptions options)
    {
        if (!options.MessagingMode.Equals("ServiceBus", StringComparison.OrdinalIgnoreCase))
        { services.AddSingleton<IEventPublisher, LoggingEventPublisher>(); return; }
        services.AddSingleton(_ => !string.IsNullOrWhiteSpace(options.ServiceBusConnectionString)
            ? new ServiceBusClient(options.ServiceBusConnectionString)
            : new ServiceBusClient(options.ServiceBusNamespace
                ?? throw new InvalidOperationException("ServiceBusNamespace is required."),
                new DefaultAzureCredential()));
        services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
    }

    private static void AddStorage(IServiceCollection services, InfrastructureOptions options)
    {
        if (!options.StorageMode.Equals("Blob", StringComparison.OrdinalIgnoreCase))
        { services.AddSingleton<IReceiptStore, LocalReceiptStore>(); return; }
        services.AddSingleton(_ => !string.IsNullOrWhiteSpace(options.BlobConnectionString)
            ? new BlobServiceClient(options.BlobConnectionString)
            : new BlobServiceClient(new Uri(options.BlobServiceUri
                ?? throw new InvalidOperationException("BlobServiceUri is required.")),
                new DefaultAzureCredential()));
        services.AddSingleton<IReceiptStore, BlobReceiptStore>();
    }
}
