# ADR 0003: Partition workflow ordering with Service Bus sessions

- Status: Accepted
- Date: 2026-08-13

## Context

Events for one order must not be applied out of order, while unrelated orders
should process concurrently.

## Decision

Set `SessionId` to the order ID for workflow events and use a session-enabled
subscription. Analytics receives an independent subscription and may process
without the workflow's ordering guarantees.

## Consequences

One order is serialized without serializing the entire system. A hot session can
limit that order's throughput, and every workflow message must include a valid
session identifier.
