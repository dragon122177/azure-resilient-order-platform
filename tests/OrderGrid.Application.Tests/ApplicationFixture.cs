using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderGrid.Application.Operations;
using OrderGrid.Application.Orders;
using OrderGrid.Application.Workflows;
using OrderGrid.Infrastructure.Context;
using OrderGrid.Infrastructure.Persistence;
namespace OrderGrid.Application.Tests;
internal sealed class ApplicationFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private ApplicationFixture(SqliteConnection connection, OrderGridDbContext db,
        RequestContext context, TestClock clock)
    {
        _connection = connection; Db = db; Clock = clock;
        var orders = new OrderRepository(db); var inventory = new InventoryRepository(db);
        Operations = new OperationsRepository(db);
        Orders = new OrderService(orders, inventory, Operations, db, context, clock);
        Workflow = new OrderWorkflow(orders, inventory, Operations, db, context, clock);
        Idempotency = new IdempotencyService(Operations, db, context, clock);
    }
    public OrderGridDbContext Db { get; }
    public TestClock Clock { get; }
    public OrderService Orders { get; }
    public OrderWorkflow Workflow { get; }
    public OperationsRepository Operations { get; }
    public IdempotencyService Idempotency { get; }
    public static async Task<ApplicationFixture> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        var context = new RequestContext(); context.Set("demo", "test-user", "test-correlation");
        var db = new OrderGridDbContext(new DbContextOptionsBuilder<OrderGridDbContext>()
            .UseSqlite(connection).Options, context); await db.Database.EnsureCreatedAsync();
        return new(connection, db, context, new TestClock(new DateTimeOffset(2026, 8, 12, 12, 0, 0, TimeSpan.Zero)));
    }
    public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
}
