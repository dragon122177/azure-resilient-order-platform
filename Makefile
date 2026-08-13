.PHONY: restore build test run-api run-worker web-install web-dev web-build emulators-up emulators-down verify

restore:
	dotnet restore OrderGrid.slnx

build:
	dotnet build OrderGrid.slnx --configuration Release --no-restore

test:
	dotnet test OrderGrid.slnx --configuration Release --no-build --collect:"XPlat Code Coverage"

run-api:
	dotnet run --project src/OrderGrid.Api

run-worker:
	dotnet run --project src/OrderGrid.Worker

web-install:
	npm --prefix web ci

web-dev:
	npm --prefix web run dev

web-build:
	npm --prefix web run build

emulators-up:
	docker compose up -d

emulators-down:
	docker compose down

verify: restore build test web-install web-build
	npm --prefix web test
