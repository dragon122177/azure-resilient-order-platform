using Microsoft.Extensions.Hosting;
using OrderGrid.Infrastructure;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) => services.AddInfrastructure(context.Configuration))
    .Build();
await host.RunAsync();
