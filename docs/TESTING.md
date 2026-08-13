# Testing strategy

Tests are organized by failure cost and feedback speed.

| Layer | What it proves |
|---|---|
| Domain | money/inventory invariants and legal order transitions |
| Application | idempotency, orchestration, compensation, tenant behavior |
| API | routing, authentication context, validation, replay, Problem Details |
| Web | formatting and operator-facing utility behavior |
| CI | restore, release build, tests, Bicep formatting/build, container builds |

## Commands

```bash
dotnet test OrderGrid.slnx --configuration Release
npm --prefix web test
npm --prefix web run build
az bicep build --file infra/main.bicep
```

`make verify` runs the local aggregate. API/application integration tests use an
isolated SQLite database so they exercise EF mappings and real transactions rather
than mocking the persistence boundary.

## Missing before production

- Azure-hosted integration tests for Service Bus sessions, DLQ, identity, and Blob.
- Contract compatibility tests across independently deployed consumers.
- Load/soak tests that validate concurrency and autoscaling assumptions.
- Failure injection around SQL, Service Bus, and process termination.
- Security tests for cross-tenant enumeration and privilege escalation.
- Backup/restore and regional recovery exercises.

Coverage is evidence, not the goal. Tests prioritize invariants and recovery paths.
