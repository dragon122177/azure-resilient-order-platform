# Roadmap

This list separates demonstrable baseline behavior from potential next steps.

## Reliability

- [ ] Lease/claim outbox rows for safe horizontal dispatcher scaling.
- [ ] Add schema-version compatibility tests and controlled DLQ replay tooling.
- [ ] Add Azure-hosted failure injection and load/soak scenarios.
- [ ] Measure and publish environment-specific SLIs before defining SLOs.

## Security

- [ ] Entra-only SQL, private endpoints, VNet integration, APIM/Front Door/WAF.
- [ ] Signed images, SBOM, provenance and deployment policy gates.
- [ ] Cross-tenant fuzzing and an external threat-model review.

## Delivery and operations

- [ ] Separate migration job with expand/contract database changes.
- [ ] Preview environments and promotion by verified image digest.
- [ ] Operational dashboard, synthetic journey, backup/restore drill evidence.

## Product

- [ ] Real payment-provider port with webhook idempotency and reconciliation.
- [ ] Versioned carrier integration and fulfillment exception workflow.
- [ ] Accessible operator actions with approval/audit controls.

Items are intentionally unchecked until implemented and verified.
