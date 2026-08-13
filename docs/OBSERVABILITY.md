# Observability

Every request has a correlation ID. It is copied into audit entries, domain-event
envelopes, Service Bus application properties, structured logs, and telemetry.
That common key allows an operator to move from an HTTP error to the related
workflow attempts without logging customer payloads.

## Signals

| Signal | Examples | Action |
|---|---|---|
| Logs | transition, publish attempt, correlation ID | Diagnose one workflow |
| Metrics | outbox age, DLQ depth, error rate, latency | Detect service degradation |
| Traces | HTTP, EF Core, Azure SDK dependencies | Locate latency/failure boundary |
| Audit | actor, action, tenant, resource | Explain business/admin changes |

## Alert design

Alerts in Bicep cover application exceptions and unhealthy revisions. Production
should add outbox age, DLQ growth, database saturation, Service Bus throttling, and
synthetic create/read journeys. Each page needs an owner, threshold rationale,
dashboard, and runbook. Avoid alerts that cannot lead to a concrete action.

## Useful KQL starting points

```kusto
AppTraces
| where TimeGenerated > ago(1h)
| where Properties["CorrelationId"] == "<correlation-id>"
| order by TimeGenerated asc
```

```kusto
AppExceptions
| where TimeGenerated > ago(24h)
| summarize failures=count() by ProblemId, bin(TimeGenerated, 15m)
```

Names vary with the Azure Monitor/OpenTelemetry schema and workspace configuration;
confirm fields against the deployed resource before turning these into alerts.
