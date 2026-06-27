## Context

Greenfield .NET 10 API project that needs a Clean Architecture skeleton as the starting point for the WEX Transaction challenge. The physical directory structure, solution file, and all project references must be in place before any feature implementation can begin.

## Goals / Non-Goals

**Goals:**
- Scaffold all Clean Architecture layer projects under `src/WexTransaction/`
- Wire project references correctly (Api → Application → Domain; Infrastructure → Application + Domain)
- Set up `docker-compose.yml` with a Postgres 15.18-alpine3.23 service
- Create `.gitignore`, `.editorconfig` at the repository root
- Create the `tests/` project (xUnit) with references to the Application and Domain layers
- Install NuGet packages for each layer (EF Core, Refit, Polly) without writing any usage code
- Produce a buildable solution with zero application logic

**Non-Goals:**
- Implementing any business logic, endpoints, or database migrations
- Configuring CI/CD pipelines
- Writing EF Core migrations, DbContext configuration, or HTTP client implementations

## Decisions

### 1. Clean Architecture layer mapping
- **Domain** (`WexTransaction.Domain`): Entities, value objects, domain exceptions — no dependencies on other layers
- **Application** (`WexTransaction.Application`): Use-case interfaces, DTOs, service contracts — depends only on Domain
- **Infrastructure** (`WexTransaction.Infrastructure`): EF Core `DbContext`, Postgres provider, Refit HTTP clients, Polly policies — depends on Application + Domain
- **Api** (`WexTransaction.Api`): ASP.NET Core Web API, controllers, DI registration — depends on Application + Infrastructure

This direction of dependencies keeps the domain free from framework concerns.

### 2. Solution file format
Using `.slnx` (the new XML-based solution format introduced in .NET 9+) as required by the project spec. This is the modern alternative to `.sln` and is natively supported by `dotnet` CLI in .NET 10.

### 3. Database volume mount
Postgres data will be stored under the `database/` folder at the repository root via a Docker bind mount. This makes the data visible and easy to reset during development without any Docker volume management.

### 4. Tests project location
The `WexTransaction.Tests` project lives under `tests/` (not inside `src/`) to cleanly separate production and test code. It references Application and Domain layers for unit testing.

## Risks / Trade-offs

- `.slnx` is newer and some IDE tooling (older Rider versions) may have limited support → Mitigation: Document the minimum required .NET SDK version (10.0) in the README
- Binding-mounting `database/` means Postgres data is owned by the postgres user in the container; the folder must exist before `docker compose up` → Mitigation: Track `database/.gitkeep` in git using `database/*` + `!database/.gitkeep` in `.gitignore` so the folder is always present after a clone
- Docker image tag `postgres:15.18-alpine3.23` must be verified on Docker Hub before implementation; if the tag does not exist, use the closest available patch tag (e.g., `postgres:15-alpine3.23`) → Mitigation: task 3.4 validates this explicitly before the build step
