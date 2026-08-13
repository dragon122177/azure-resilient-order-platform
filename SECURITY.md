# Security policy

## Reporting a vulnerability

Do not open a public issue for a suspected vulnerability. Use GitHub's private
vulnerability reporting for this repository and include affected components,
reproduction steps, impact, and any proposed mitigation. Please allow time for
triage before public disclosure.

## Supported scope

This repository is a portfolio reference implementation, not a hosted service.
Security reports should concern the current `main` branch. The demo authentication
mode, deterministic payment simulator, sample secrets, and public-image bootstrap
are documented local-development choices and must not be enabled in production.

## Production checklist

- Set `Authentication__Mode=EntraId` and configure the expected issuer/audience.
- Replace SQL password bootstrap with Entra-only database authentication.
- Use private endpoints, network restrictions, and a controlled ingress tier.
- Rotate any value that has left its approved secret store.
- Run dependency, container, IaC, DAST, and penetration testing in the target environment.
- Review managed-identity role assignments and application roles before each release.
