# Local development

## Prerequisites

- .NET SDK matching `global.json`
- Node.js 24 and npm
- Docker Desktop or Docker Engine (optional emulators)
- Azure CLI with Bicep (only for infrastructure validation/deployment)

## Fast path

```bash
dotnet restore OrderGrid.slnx
dotnet run --project src/OrderGrid.Api
dotnet run --project src/OrderGrid.Worker
npm --prefix web ci
npm --prefix web run dev
```

Run each long-lived process in a separate terminal. The default profile uses a
SQLite file, local event publishing, local receipt files, seeded synthetic
inventory, and demo authentication. The API is available at `http://localhost:8080`
when `ASPNETCORE_URLS=http://localhost:8080` is set; Vite defaults to port 5173.
The committed launch profiles set the .NET environment to `Development`; do not
override it with `Production` unless the required Azure settings are present.

## Optional emulators

```bash
docker compose up -d
```

This starts Microsoft's Service Bus emulator and Azurite. Copy the non-secret
settings from `.env.example` into your shell and switch messaging/storage modes.
The emulator is for integration feedback, not a substitute for Azure-hosted tests.

## Useful scenarios

- Normal flow: use `samples/create-order.json`.
- Idempotent replay: send identical body and key twice.
- Key misuse: change body while reusing the key and expect `409`.
- Decline/compensation: set customer email to contain `decline`.
- Concurrency: submit parallel orders against the same limited SKU.

## Cleanup

Stop processes and `docker compose down`. Local databases, receipts, build output,
coverage, `.env`, and package directories are ignored by Git.
