# ADR 0004: Prefer managed identity and CI workload federation

- Status: Accepted
- Date: 2026-08-13

## Context

Long-lived Azure credentials in applications or CI create rotation and disclosure
risk.

## Decision

Use user-assigned managed identities for runtime resources with scoped RBAC. Use a
GitHub-to-Entra federated credential for deployment. Store unavoidable bootstrap
secrets in Key Vault/GitHub environments and plan their removal.

## Consequences

There are fewer credentials to distribute and rotate. Role propagation, federation
conditions, and local developer authentication require explicit setup and testing.
