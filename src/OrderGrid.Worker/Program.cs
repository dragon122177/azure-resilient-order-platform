using Azure.Monitor.OpenTelemetry.AspNetCore;
using OrderGrid.Application;
using OrderGrid.Infrastructure;
using OrderGrid.Infrastructure.Persistence;
using OrderGrid.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IEventRouter, OrderEventRouter>();
builder.Services.AddHostedService<OutboxDispatcherWorker>();
builder.Services.AddHostedService<ServiceBusConsumerWorker>();
builder.Services.AddHostedService<AnalyticsProjectionWorker>();
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
var host = builder.Build();
var options = host.Services.GetRequiredService<InfrastructureOptions>();
if (options.InitializeDatabase)
{
    await using var scope = host.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>()
        .InitializeAsync(options.SeedDemoData, CancellationToken.None);
}
await host.RunAsync();
