# Azure deployment

The workflow in `.github/workflows/deploy.yml` uses GitHub OIDC. It validates the
Bicep template, deploys the baseline, builds commit-SHA-tagged API/worker images,
pushes them to ACR, updates immutable Container Apps revisions, and checks health.

## One-time setup

1. Create an Azure subscription/resource group and a narrowly scoped deployment identity.
2. Add a federated credential restricted to this repository/environment/branch.
3. Configure GitHub environment variables for tenant, subscription, client ID,
   location, resource group, and environment name.
4. Store the bootstrap SQL administrator password as an environment secret.
5. Add reviewers and branch protection to the production GitHub environment.

Never paste Azure client secrets into Actions; OIDC exists to remove that need.

## Manual validation

```bash
az bicep build --file infra/main.bicep
az deployment group what-if \
  --resource-group ordergrid-dev-rg \
  --template-file infra/main.bicep \
  --parameters environmentName=dev sqlAdministratorPassword='<secure-value>'
```

Review deletions, RBAC changes, networking, public access, SKU changes, and secret
outputs before applying. Use unique environment names because globally scoped
resource names are derived from the deployment identity.

## Promotion and rollback

Images use the immutable Git commit SHA. Promote the same digest across environments
after checks rather than rebuilding. Container Apps keeps revisions; rollback means
moving traffic to a previously verified revision while diagnosing the new one.
Database changes must be backward compatible for at least the rollback window.

## Production hardening backlog

- Private endpoints, VNet integration, controlled egress, APIM/Front Door/WAF.
- Entra-only SQL admin and workload authentication; remove password bootstrap.
- Zone/geo redundancy chosen from measured RPO/RTO and budget.
- Separate migration job, signed images, SBOM/provenance, policy enforcement.
- Azure-hosted smoke, load, chaos, restore, and security tests.
- Measured SLOs, alert tuning, support ownership, and capacity reviews.
