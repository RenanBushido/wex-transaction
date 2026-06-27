## Why

The WEX Transaction project needs a Clean Architecture skeleton to serve as the foundation for implementing the purchase transaction API. Without a proper scaffolded structure, development cannot begin in an organized and maintainable way.

## What Changes

- Create the .NET 10 solution file (`WexTransaction.slnx`) inside `src/WexTransaction/`
- Create the `WexTransaction.Api` project (Presentation layer — controllers, middleware, DI setup)
- Create the `WexTransaction.Application` project (Application layer — use cases, interfaces, DTOs)
- Create the `WexTransaction.Domain` project (Domain layer — entities, value objects, domain rules)
- Create the `WexTransaction.Infrastructure` project (Infrastructure layer — EF Core, Postgres, external HTTP clients)
- Create the `WexTransaction.Tests` project inside `tests/` (xUnit unit tests)
- Create `.gitignore` and `.editorconfig` files at the repository root
- Create `docker-compose.yml` with Postgres 15.18-alpine3.23 service
- Create `database/` folder for Postgres data persistence

## Capabilities

### New Capabilities

- `project-skeleton`: Full Clean Architecture .NET 10 solution structure with all layer projects, solution file, docker-compose, and developer tooling files

### Modified Capabilities

## Impact

- Establishes the physical directory structure under `src/`, `tests/`, and `database/`
- All future implementation work depends on this skeleton being in place
- No existing code is affected (greenfield project)
