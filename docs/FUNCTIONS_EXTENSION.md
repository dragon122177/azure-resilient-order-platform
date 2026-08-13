# Optional Azure Functions extension

The default deployment runs the workflow consumer and analytics projection in the
worker. `OrderGrid.Functions` demonstrates the isolated .NET Functions programming
model without creating two active consumers for the same responsibility.

It contains:

- `DeliveredOrderProjection`: a Service Bus trigger example for delivered events.
- `ReconciliationSweep`: a timer that reports stale pending outbox work.

Enable/deploy it only after assigning a distinct subscription and reviewing RBAC,
concurrency, retry, host storage, and monitoring. Never point both the worker and
Function at the same subscription unless competing-consumer behavior is intended.

Local settings belong in `local.settings.json`, which is ignored. The committed
`local.settings.example.json` contains names/placeholders only.

For production, add Functions resources to Bicep, package immutable artifacts,
configure managed identity instead of connection-string secrets, add trigger-level
integration tests, and give the extension its own dashboards and runbooks.
