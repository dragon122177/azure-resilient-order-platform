using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderGrid.Domain.Inventory;
namespace OrderGrid.Infrastructure.Persistence;
public sealed class DatabaseInitializer(OrderGridDbContext db, ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(bool seed, CancellationToken token)
    {
        await db.Database.EnsureCreatedAsync(token);
        if (!seed || await db.Inventory.AnyAsync(token)) return;
        var now = DateTimeOffset.UtcNow;
        InventoryItem[] items =
        [
            new("demo", "AZ-100", "Azure Architecture Workbook", 250, now),
            new("demo", "DOTNET-10", ".NET Cloud Engineering Guide", 180, now),
            new("demo", "SB-200", "Service Bus Reliability Kit", 120, now),
            new("demo", "OBS-300", "Observability Field Manual", 90, now)
        ];
        await db.Inventory.AddRangeAsync(items, token);
        await db.SaveChangesAsync(token);
        logger.LogInformation("Seeded {Count} demo inventory records", items.Length);
    }
}
