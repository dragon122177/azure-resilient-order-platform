# ADR 0002: Use an outbox and idempotent consumers

- Status: Accepted
- Date: 2026-08-13

## Context

Azure SQL and Service Bus do not share the application's local transaction. A
failure between saving state and publishing can lose or duplicate work.

## Decision

Persist domain-event envelopes in an outbox alongside business changes. A worker
publishes pending rows and records completion. Consumers persist an inbox key with
their state transition and tolerate repeated delivery.

## Consequences

No order event is intentionally lost after a successful database commit, but
duplicates remain possible. Operations must monitor outbox age and DLQ depth, and
handlers must be deterministic and idempotent.
