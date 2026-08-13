# Cost model

This repository intentionally avoids quoting a fixed monthly total: Azure prices
change by region, currency, reservation, redundancy, egress, and usage. Use the
Azure Pricing Calculator with the intended region and validate actual cost data.

## Main cost drivers

| Component | Primary driver | Development control |
|---|---|---|
| Container Apps | vCPU/GB-seconds and requests | scale-to-zero where safe, small limits |
| Azure SQL | compute tier, storage, backup | low non-production tier, scheduled lifecycle |
| Service Bus | tier and operations | Basic cannot support topics; use smallest valid tier |
| Blob Storage | capacity, transactions, egress | lifecycle rules and short demo retention |
| Log Analytics | ingestion and retention | sampling, filters, explicit retention |
| ACR | registry tier and stored layers | delete unreferenced SHA images |
| Networking | gateways, private endpoints, egress | include security topology in budget |

## Cost questions before production

- What peak and steady order/message rates were load-tested?
- How much telemetry is generated per order, and what must be retained?
- What availability/recovery target justifies redundancy cost?
- Can cold starts or scale-to-zero meet the user experience?
- Which environments can be ephemeral or stopped outside working hours?

Cost optimization must not remove security controls or hide reliability risk. Tag
resources by application/environment/owner, set budgets, and investigate anomalies.
