# Contributing

Thank you for improving OrderGrid. Keep changes small, explain the operational
impact, and preserve the boundary between domain rules and Azure adapters.

## Development workflow

1. Create a branch from `main`.
2. Run `dotnet restore OrderGrid.slnx` and `npm --prefix web ci`.
3. Add or update tests for behavior changes.
4. Run `make verify` before opening a pull request.
5. Update an ADR when changing a durable architectural decision.

Commits should explain one coherent change. Never commit credentials, `.env`
files, generated build output, local databases, or production data.

## Definition of done

- Builds without warnings and all tests pass.
- Tenant boundaries and authorization policies remain explicit.
- Retries remain bounded; handlers remain idempotent.
- Telemetry contains correlation IDs but no sensitive payloads.
- Documentation and Bicep stay synchronized with runtime behavior.
