# Security design and threat model

## Trust boundaries

Internet traffic crosses the API ingress boundary. The API, worker, Service Bus,
SQL, Blob, Key Vault, registry, and CI federation are separate principals and
must receive independent, least-privilege permissions.

| Threat | Control in baseline | Production extension |
|---|---|---|
| Spoofed caller | Entra JWT issuer/audience validation | Conditional Access and APIM policies |
| Tenant data leak | Tenant context and scoped queries | RLS, adversarial tests, separate databases where needed |
| Duplicate command | Idempotency key + request hash | Quotas and abuse monitoring |
| Message replay | Inbox deduplication + state invariants | Contract versioning and replay drills |
| Credential theft | Managed identity, OIDC, Key Vault | Workload network isolation and PIM |
| Image tampering | Private ACR and SHA tags | Signing, attestations, admission policy |
| Sensitive logs | Structured metadata, no body logging | DLP rules and retention policy |
| Dependency compromise | Dependabot and CodeQL | SBOM, container scan, provenance gates |

## Authorization

The API defines separate read, write, and operations policies. Entra application
roles should map to those policies. The local demo handler exists to make the
repository runnable and is disabled by `Authentication__Mode=EntraId`.

## Secrets

No production credential belongs in Git, an image, telemetry, or deployment
output. GitHub authenticates to Azure through a federated identity. Runtime code
uses managed identity for Service Bus, Blob, Key Vault, ACR, and monitoring. The
SQL administrator password remains a bootstrap limitation in the Bicep sample;
the production target is Entra-only SQL administration and workload access.

## Residual risks

Public Container Apps ingress, database bootstrap, and public Azure endpoints keep
the reference environment inexpensive and understandable. They are not a claim of
production hardening. See `SECURITY.md` for reporting and the deployment guide for
the explicit hardening backlog.
