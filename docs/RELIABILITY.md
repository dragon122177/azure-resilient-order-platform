# Reliability model

OrderGrid distinguishes a durable guarantee from an operational aspiration. It
does not claim exactly-once delivery or a production SLO without measurements.

## Guarantees

- Order state and outbox intent commit atomically.
- API command replay has a stable response within the idempotency retention window.
- Consumers may receive messages more than once; inbox records deduplicate effects.
- Per-order Service Bus sessions preserve ordering within one order stream.
- State-machine guards reject impossible transitions.
- Inventory compensation accompanies simulated payment failure.

## Retry policy

Retries belong at transient boundaries and must be bounded. The dispatcher records
attempt count/error and delays another attempt. Service Bus applies delivery count
and a DLQ. Business conflicts, invalid contracts, and impossible transitions are
not made healthy by blind retry.

## Recovery point and time

The reference deployment inherits Azure SQL and Storage defaults; it does not make
an unverified RPO/RTO claim. A production owner must select redundancy and backup
retention, run restore exercises, record measured times, and align alerts/runbooks
with business objectives.

## Suggested service-level indicators

- Successful order creates / valid create attempts.
- p50/p95/p99 create latency.
- Oldest unpublished outbox age and pending count.
- Active messages, DLQ count, and maximum delivery count.
- Workflow time from `Submitted` to `ReadyForFulfillment`.
- Failed or stuck orders by reason and tenant.

Example starting objectives for a load-tested non-critical environment could be
99.9% successful valid creates and 99% workflow completion under two minutes. They
are hypotheses until the deployed system has traffic, load tests, and error budgets.
