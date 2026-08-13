# ADR 0001: Start with a modular monolith and asynchronous worker

- Status: Accepted
- Date: 2026-08-13

## Context

The sample needs transactional consistency, understandable local development, and
clear Azure boundaries without inventing organizational scale.

## Decision

Use one domain/application/infrastructure model and deploy separate API and worker
hosts. Keep module boundaries in code and communicate through explicit events where
asynchrony adds reliability or independent processing.

## Consequences

Order state and outbox can share a transaction and the project is easy to run. The
API and worker remain separately scalable. A future service split will require
data ownership, versioned contracts, and more operational infrastructure.
