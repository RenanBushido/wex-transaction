## 1. Repository Root Setup

- [x] 1.1 Create `database/` folder at the repository root with a `.gitkeep` file
- [x] 1.2 Create `.gitignore` at the repository root (ignore `bin/`, `obj/`, `*.user`, `.env`; for the database folder use `database/*` + `!database/.gitkeep` so the tracked empty folder is preserved after a clone)
- [x] 1.3 Create `.editorconfig` at the repository root with C# formatting rules (indent style, charset, newlines)
- [x] 1.4 Create `docker-compose.yml` with a `postgres:15.18-alpine3.23` service, bind-mounting `./database` to `/var/lib/postgresql/data` on port 5432

## 2. Solution and Project Scaffolding

- [x] 2.1 Create the directory `src/WexTransaction/`
- [x] 2.2 Create the `WexTransaction.Domain` class library project at `src/WexTransaction/WexTransaction.Domain/`
- [x] 2.3 Create the `WexTransaction.Application` class library project at `src/WexTransaction/WexTransaction.Application/` and add a project reference to `WexTransaction.Domain`
- [x] 2.4 Create the `WexTransaction.Infrastructure` class library project at `src/WexTransaction/WexTransaction.Infrastructure/` and add project references to `WexTransaction.Application` and `WexTransaction.Domain`
- [x] 2.5 Create the `WexTransaction.Api` ASP.NET Core Web API project at `src/WexTransaction/WexTransaction.Api/` and add project references to `WexTransaction.Application` and `WexTransaction.Infrastructure`
- [x] 2.6 Create `src/WexTransaction/WexTransaction.Api/appsettings.Development.json` with a placeholder connection string pointing to the local Postgres container (host `localhost`, port `5432`, database `wex`, user `postgres`)
- [x] 2.7 Install NuGet packages: `Npgsql.EntityFrameworkCore.PostgreSQL` and `Microsoft.EntityFrameworkCore.Design` in `WexTransaction.Infrastructure`; `Microsoft.EntityFrameworkCore` in `WexTransaction.Application`; `Refit` and `Polly` in `WexTransaction.Infrastructure`
- [x] 2.8 Create the `WexTransaction.slnx` solution file at `src/WexTransaction/` and add all four projects to it

## 3. Tests Project Scaffolding

- [x] 3.1 Create the directory `tests/WexTransaction.Tests/`
- [x] 3.2 Create the `WexTransaction.Tests` xUnit test project at `tests/WexTransaction.Tests/` and add project references to `WexTransaction.Application` and `WexTransaction.Domain`
- [x] 3.3 Add the `WexTransaction.Tests` project to the solution file `WexTransaction.slnx`
- [x] 3.4 Verify that the Docker image tag `postgres:15.18-alpine3.23` exists on Docker Hub (`docker pull postgres:15.18-alpine3.23`); if not found, update the tag in `docker-compose.yml` and this change's artifacts to the closest valid tag

## 4. Verify Build

- [x] 4.1 Run `dotnet build src/WexTransaction/WexTransaction.slnx` and confirm it succeeds with no errors
- [x] 4.2 Run `dotnet test` from the repository root and confirm the test project is discovered and runs without errors
