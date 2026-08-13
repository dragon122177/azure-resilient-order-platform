# Data model

The relational model keeps workflow truth, deduplication records, and publication
intent in one transactional boundary.

```mermaid
erDiagram
  ORDERS ||--|{ ORDER_ITEMS : contains
  ORDERS }o--o{ OUTBOX_MESSAGES : emits
  ORDERS }o--o{ AUDIT_ENTRIES : records
  ORDERS {
    guid Id PK
    string TenantId
    string ExternalReference
    string Status
    decimal TotalAmount
    bytes RowVersion
  }
  ORDER_ITEMS {
    guid Id PK
    guid OrderId FK
    string Sku
    int Quantity
  }
  INVENTORY {
    guid Id PK
    string TenantId
    string Sku
    int Available
    int Reserved
    bytes RowVersion
  }
  OUTBOX_MESSAGES {
    guid Id PK
    string EventType
    datetime OccurredAt
    datetime PublishedAt
  }
  INBOX_MESSAGES {
    string Consumer PK
    string MessageId PK
  }
  IDEMPOTENCY_RECORDS {
    string TenantId PK
    string Key PK
    string RequestHash
  }
```

## Invariants

- `(TenantId, ExternalReference)` is unique for orders.
- `(TenantId, Sku)` is unique for inventory.
- Available and reserved quantities cannot become negative through domain methods.
- Order totals are derived from immutable line values.
- `(Consumer, MessageId)` is the consumer deduplication boundary.
- `(TenantId, Key)` is the API idempotency boundary.
- SQL Server rowversion and SQLite concurrency tokens detect stale writes.

SQLite is used only for local/test portability. Azure SQL uses decimal precision,
rowversion, indexes, backups, and operational controls appropriate to the hosted
environment. A production system should run versioned migrations as a separate
release step instead of letting an application instance initialize the database.
