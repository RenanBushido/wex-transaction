## Purpose

Defines the structural requirements for the WexTransaction repository skeleton: the .NET solution layout, layer projects, test project, Docker Compose setup, and developer tooling files that must be present before any feature implementation begins.

---

## Requirements

### Requirement: Solution file exists
The repository SHALL contain a `WexTransaction.slnx` solution file at `src/WexTransaction/WexTransaction.slnx` that includes all layer projects and the tests project.

#### Scenario: Solution file is present at expected path
- **WHEN** the developer navigates to `src/WexTransaction/`
- **THEN** a file named `WexTransaction.slnx` SHALL exist and be parseable by `dotnet` CLI

### Requirement: Domain layer project exists
The repository SHALL contain a class library project `WexTransaction.Domain` at `src/WexTransaction/WexTransaction.Domain/` with no references to other solution projects.

#### Scenario: Domain project compiles independently
- **WHEN** `dotnet build` is run targeting `WexTransaction.Domain`
- **THEN** the project SHALL compile successfully with no errors

### Requirement: Application layer project exists
The repository SHALL contain a class library project `WexTransaction.Application` at `src/WexTransaction/WexTransaction.Application/` that references only `WexTransaction.Domain`.

#### Scenario: Application project references Domain
- **WHEN** the Application `.csproj` is inspected
- **THEN** it SHALL contain a `<ProjectReference>` to `WexTransaction.Domain` and no reference to Infrastructure or Api

### Requirement: Infrastructure layer project exists
The repository SHALL contain a class library project `WexTransaction.Infrastructure` at `src/WexTransaction/WexTransaction.Infrastructure/` that references `WexTransaction.Application` and `WexTransaction.Domain`.

#### Scenario: Infrastructure project references Application and Domain
- **WHEN** the Infrastructure `.csproj` is inspected
- **THEN** it SHALL contain `<ProjectReference>` entries for both `WexTransaction.Application` and `WexTransaction.Domain`

### Requirement: Api layer project exists
The repository SHALL contain an ASP.NET Core Web API project `WexTransaction.Api` at `src/WexTransaction/WexTransaction.Api/` that references `WexTransaction.Application` and `WexTransaction.Infrastructure`.

#### Scenario: Api project references Application and Infrastructure
- **WHEN** the Api `.csproj` is inspected
- **THEN** it SHALL contain `<ProjectReference>` entries for `WexTransaction.Application` and `WexTransaction.Infrastructure`

#### Scenario: Api project starts without errors
- **WHEN** `dotnet run` is executed in `WexTransaction.Api`
- **THEN** the application SHALL start and listen on an HTTP port with no build errors

### Requirement: Tests project exists
The repository SHALL contain an xUnit test project `WexTransaction.Tests` at `tests/WexTransaction.Tests/` that references `WexTransaction.Application` and `WexTransaction.Domain`.

#### Scenario: Tests project compiles and runs
- **WHEN** `dotnet test` is executed from the repository root
- **THEN** the test runner SHALL discover and execute tests with no build errors

### Requirement: Docker Compose configures Postgres
The repository SHALL contain a `docker-compose.yml` at the repository root that defines a `postgres` service using image `postgres:15.18-alpine3.23`, with a bind mount from `./database` to `/var/lib/postgresql/data`.

#### Scenario: Postgres container starts via docker-compose
- **WHEN** `docker compose up -d` is run
- **THEN** the postgres service SHALL start and accept connections on port 5432

### Requirement: Developer tooling files exist
The repository root SHALL contain a `.gitignore` file (ignoring `bin/`, `obj/`, `.env`, `*.user`; using `database/*` with `!database/.gitkeep` to track the empty folder while ignoring Postgres data files) and an `.editorconfig` file with C# formatting rules.

#### Scenario: gitignore excludes build artifacts
- **WHEN** `git status` is run after building the solution
- **THEN** `bin/` and `obj/` directories SHALL NOT appear as untracked files

#### Scenario: database folder is tracked but its contents are ignored
- **WHEN** `git status` is run on a fresh clone
- **THEN** `database/.gitkeep` SHALL appear as a tracked file and any files inside `database/` added by Postgres SHALL NOT appear as untracked

#### Scenario: editorconfig is present
- **WHEN** the repository root is listed
- **THEN** a file named `.editorconfig` SHALL exist

### Requirement: Api project has development connection string placeholder
The `WexTransaction.Api` project SHALL contain an `appsettings.Development.json` file with a placeholder connection string for the local Postgres container.

#### Scenario: Development settings file exists
- **WHEN** the `WexTransaction.Api` project folder is listed
- **THEN** a file named `appsettings.Development.json` SHALL exist with a `ConnectionStrings` section referencing the local Postgres container
