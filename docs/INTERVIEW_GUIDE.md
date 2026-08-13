# Interview guide

## Strong opening

"OrderGrid is a reference platform I built to demonstrate reliable order workflows
on Azure. The core problem is keeping database state and messages consistent under
retries and partial failures. I used a transactional outbox, idempotent HTTP
commands, inbox deduplication, Service Bus sessions, and a guarded state machine.
The project runs locally, deploys through Bicep/OIDC, and documents production gaps."

## Design questions

**Why Service Bus instead of Event Grid?** The workflow needs durable competing
consumers, ordered sessions, delivery attempts, and a DLQ. Event Grid is excellent
for lightweight event distribution but is not the same queueing contract.

**Why one database?** It gives a clear transaction boundary while the domain and
team are small. Split ownership only when independent scale/release/availability
needs outweigh distributed consistency and operations cost.

**What happens if publish succeeds but marking the outbox fails?** The event may be
published again. That is expected; stable message IDs and consumer inbox records
make repeated delivery safe.

**How do you prevent overselling?** Domain checks plus optimistic concurrency. A
high-contention SKU may need bounded retries, pessimistic locking, partitioned
reservation ownership, or a dedicated inventory service after measurement.

**What is the hardest production gap?** Identity/network/database lifecycle and
proving behavior under failure. The design names those gaps instead of claiming
that an IaC deployment alone makes it production-ready.

## STAR material to personalize

- Situation: duplicate/late work or manual operational uncertainty.
- Task: build a traceable, retry-safe path with clear ownership.
- Action: idempotency, transaction/outbox, message sessions, inbox, audit and alerts.
- Result: use only results you actually measured. For this portfolio project,
  discuss passing tests and demonstrated failure scenarios—not invented customers.

## Whiteboard checklist

Identify the consistency boundary, delivery semantics, idempotency scope, tenant
source, authorization, failure classification, retry budget, DLQ recovery,
observability key, deployment identity, cost driver, and honest next step.
