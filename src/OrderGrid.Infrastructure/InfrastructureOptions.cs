namespace OrderGrid.Infrastructure;
public sealed class InfrastructureOptions
{
    public const string SectionName = "Infrastructure";
    public string DatabaseProvider { get; init; } = "Sqlite";
    public string DatabaseConnectionString { get; init; } = "Data Source=ordergrid.db";
    public bool InitializeDatabase { get; init; } = true;
    public bool SeedDemoData { get; init; } = true;
    public string MessagingMode { get; init; } = "Local";
    public string? ServiceBusConnectionString { get; init; }
    public string? ServiceBusNamespace { get; init; }
    public string ServiceBusTopic { get; init; } = "order-events";
    public string ServiceBusSubscription { get; init; } = "orchestrator";
    public string ServiceBusAnalyticsSubscription { get; init; } = "analytics";
    public string StorageMode { get; init; } = "Local";
    public string? BlobConnectionString { get; init; }
    public string? BlobServiceUri { get; init; }
    public string ReceiptContainer { get; init; } = "order-receipts";
    public string LocalStoragePath { get; init; } = "./artifacts/receipts";
}
