# Five-minute demo

## 0:00–0:45 — Problem and topology

Explain that retries and partial failures make a cloud order flow harder than CRUD.
Use the README diagram to identify API, transaction, outbox, Service Bus, consumers,
and telemetry. State clearly that payment and data are synthetic.

## 0:45–1:45 — Domain and transaction

Open `Order.cs`, `OrderWorkflow.cs`, and `OrderGridDbContext.cs`. Show guarded
transitions, payment-decline compensation, and domain events converted to outbox
records by `SaveChangesAsync`.

## 1:45–2:45 — Duplicate safety

Create an order from `samples/create-order.json`. Repeat it with the same
`Idempotency-Key` and point out the replay header. Explain that consumer inbox
deduplication handles the asynchronous side of at-least-once delivery.

## 2:45–3:35 — Operations

Open the React console or operations endpoints. Show status counts, inventory,
audit/correlation IDs, and the explicit API/demo-data label. Mention DLQ and outbox
age as production paging signals.

## 3:35–4:25 — Azure and security

Open `infra/main.bicep` and modules. Highlight managed identities, scoped RBAC,
Key Vault, SHA-tagged images, Application Insights, and GitHub OIDC.

## 4:25–5:00 — Evidence and limits

Show the CI workflow and test suites. End with the production-hardening list:
private networking, Entra-only SQL, migration job, load/chaos/restore/security
tests, and measured SLOs. Honest limitations strengthen the design discussion.
