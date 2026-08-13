using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderGrid.Application.Abstractions;
using OrderGrid.Domain.Common;
using OrderGrid.Domain.Inventory;
using OrderGrid.Domain.Operations;
using OrderGrid.Domain.Orders;

namespace OrderGrid.Infrastructure.Persistence;

public sealed class OrderGridDbContext(DbContextOptions<OrderGridDbContext> options,
    IRequestContext context) : DbContext(options), IUnitOfWork
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureOrders(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureOperations(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        var events = ChangeTracker.Entries<AggregateRoot>()
            .SelectMany(entry => entry.Entity.DequeueDomainEvents()).ToArray();
        foreach (var domainEvent in events)
        {
            OutboxMessages.Add(new OutboxMessage(domainEvent.EventId, context.TenantId,
                domainEvent.EventType,
                JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions),
                domainEvent.OccurredAt, context.CorrelationId));
        }
        return await base.SaveChangesAsync(token);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation,
        CancellationToken token)
    {
        if (Database.CurrentTransaction is not null) { await operation(token); return; }
        await using var transaction = await Database.BeginTransactionAsync(token);
        try { await operation(token); await transaction.CommitAsync(token); }
        catch { await transaction.RollbackAsync(token); throw; }
    }

    private void ConfigureOrders(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<Order>();
        order.ToTable("orders"); order.HasKey(x => x.Id);
        order.HasIndex(x => new { x.TenantId, x.ExternalReference }).IsUnique();
        order.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
        order.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        order.Property(x => x.ExternalReference).HasMaxLength(100).IsRequired();
        order.Property(x => x.CustomerEmail).HasMaxLength(254).IsRequired();
        order.Property(x => x.Recipient).HasMaxLength(120).IsRequired();
        order.Property(x => x.AddressLine1).HasMaxLength(160).IsRequired();
        order.Property(x => x.AddressLine2).HasMaxLength(160);
        order.Property(x => x.City).HasMaxLength(100).IsRequired();
        order.Property(x => x.Region).HasMaxLength(100);
        order.Property(x => x.PostalCode).HasMaxLength(24).IsRequired();
        order.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
        order.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        order.Property(x => x.TotalAmount).HasPrecision(18, 2);
        order.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        order.Property(x => x.PaymentReference).HasMaxLength(120);
        order.Property(x => x.Carrier).HasMaxLength(80);
        order.Property(x => x.TrackingNumber).HasMaxLength(120);
        order.Property(x => x.FailureReason).HasMaxLength(500);
        UtcTimestamp(order.Property(x => x.CreatedAt));
        UtcTimestamp(order.Property(x => x.UpdatedAt));
        Concurrency(order.Property(x => x.RowVersion));
        order.Ignore(x => x.DomainEvents); order.Ignore(x => x.Total); order.Ignore(x => x.ShippingAddress);
        order.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);

        var item = modelBuilder.Entity<OrderItem>();
        item.ToTable("order_items"); item.HasKey(x => x.Id);
        item.HasIndex(x => new { x.OrderId, x.Sku });
        item.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        item.Property(x => x.Name).HasMaxLength(160).IsRequired();
        item.Property(x => x.UnitPriceAmount).HasPrecision(18, 2);
        item.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        item.Ignore(x => x.UnitPrice); item.Ignore(x => x.LineTotal);
    }

    private void ConfigureInventory(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<InventoryItem>();
        entity.ToTable("inventory"); entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.TenantId, x.Sku }).IsUnique();
        entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        UtcTimestamp(entity.Property(x => x.UpdatedAt));
        Concurrency(entity.Property(x => x.RowVersion));
    }

    private void ConfigureOperations(ModelBuilder modelBuilder)
    {
        var outbox = modelBuilder.Entity<OutboxMessage>();
        outbox.ToTable("outbox_messages"); outbox.HasKey(x => x.Id);
        outbox.HasIndex(x => new { x.PublishedAt, x.OccurredAt });
        outbox.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        outbox.Property(x => x.EventType).HasMaxLength(160).IsRequired();
        outbox.Property(x => x.CorrelationId).HasMaxLength(128).IsRequired();
        outbox.Property(x => x.Payload).IsRequired(); outbox.Property(x => x.LastError).HasMaxLength(2_000);
        UtcTimestamp(outbox.Property(x => x.OccurredAt));

        var inbox = modelBuilder.Entity<InboxMessage>();
        inbox.ToTable("inbox_messages"); inbox.HasKey(x => new { x.Consumer, x.MessageId });
        inbox.Property(x => x.Consumer).HasMaxLength(120); inbox.Property(x => x.MessageId).HasMaxLength(160);
        UtcTimestamp(inbox.Property(x => x.ProcessedAt));

        var audit = modelBuilder.Entity<AuditEntry>();
        audit.ToTable("audit_entries"); audit.HasKey(x => x.Id);
        audit.HasIndex(x => new { x.TenantId, x.OccurredAt });
        audit.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        audit.Property(x => x.Actor).HasMaxLength(160).IsRequired();
        audit.Property(x => x.Action).HasMaxLength(120).IsRequired();
        audit.Property(x => x.ResourceType).HasMaxLength(80).IsRequired();
        audit.Property(x => x.ResourceId).HasMaxLength(160).IsRequired();
        audit.Property(x => x.CorrelationId).HasMaxLength(128).IsRequired();
        audit.Property(x => x.Details).HasMaxLength(2_000);
        UtcTimestamp(audit.Property(x => x.OccurredAt));

        var idem = modelBuilder.Entity<IdempotencyRecord>();
        idem.ToTable("idempotency_records"); idem.HasKey(x => new { x.TenantId, x.Key });
        idem.HasIndex(x => x.ExpiresAt); idem.Property(x => x.TenantId).HasMaxLength(64);
        idem.Property(x => x.Key).HasMaxLength(160); idem.Property(x => x.RequestHash).HasMaxLength(64);
        idem.Property(x => x.ResponseBody).IsRequired();
        UtcTimestamp(idem.Property(x => x.CreatedAt)); UtcTimestamp(idem.Property(x => x.ExpiresAt));
    }

    private void UtcTimestamp(PropertyBuilder<DateTimeOffset> property)
    {
        if (Database.IsSqlite()) property.HasConversion(
            value => value.UtcDateTime,
            value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
    }

    private void Concurrency(PropertyBuilder<byte[]> property)
    {
        if (Database.IsSqlServer()) property.IsRowVersion();
        else property.IsConcurrencyToken().ValueGeneratedNever();
    }
}
