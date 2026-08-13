# Operational runbooks

Start every incident by recording UTC start time, environment, affected tenants,
symptoms, deployment revision, and one correlation/order ID. Preserve evidence and
avoid editing production rows by hand.

## Orders are not advancing

1. Check API/worker revision health and recent deployments.
2. Inspect oldest outbox age and last error.
3. Check Service Bus active, scheduled, and dead-letter counts by subscription.
4. Trace one correlation ID through outbox, message, inbox, and audit.
5. Classify transient infrastructure failure versus deterministic contract failure.
6. Restore dependency health or roll back; do not repeatedly replay poison messages.
7. Replay a quarantined message only after fixing the cause and confirming idempotency.

## Dead-letter queue growth

1. Pause any automated replay.
2. Sample reason, event type, schema version, delivery count, and producing revision.
3. If one release introduced the errors, stop/roll back that producer or consumer.
4. Patch and test the handler against sanitized examples.
5. Replay a small canary batch, watch effects and inbox records, then expand.

## Database saturation

1. Confirm connection, CPU, IO, blocking, and query-duration signals.
2. Reduce optional workloads and consumer concurrency before scaling blindly.
3. Identify tenant/query/index causing pressure; preserve the execution plan.
4. Scale temporarily if needed, then fix query shape/indexing and verify regression tests.

## Suspected tenant isolation incident

1. Treat as a security incident and restrict affected access.
2. Preserve audit/identity/telemetry evidence without copying customer payloads.
3. Identify query, policy, tenant, actor, and time window.
4. Revoke/limit access, patch the boundary, and test for related enumeration paths.
5. Follow the organization's disclosure and legal process.

After mitigation, write a blameless review with detection gap, contributing factors,
customer impact, corrective owners, and deadlines.
