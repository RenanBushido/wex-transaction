## Why

The current Infrastructure layer is a single monolithic project mixing database concerns (EF Core, DbContext, migrations) with external API integrations (Refit clients for Treasury API). Separating these into distinct projects improves modularity, maintainability, and follows the principle of single responsibility. This structure enables independent scaling of infrastructure dependencies and clearer dependency graphs.

## What Changes

- **Remove**: `WexTransaction.Infrastructure` (raw project with Class1.cs stub)
- **Create**: `WexTransaction.Infra.Database` project for EF Core, DbContext, repositories, migrations
- **Create**: `WexTransaction.Infra.Services.RatesExchange` project for Refit client integrations with Treasury API
- **Update**: Solution file (.slnx) to reference new projects
- **Update**: API project dependencies to reference the new infrastructure projects
- **Establish**: Extension pattern in each infrastructure project following the Extensions folder convention

## Capabilities

### New Capabilities

- `infra-database`: EF Core persistence layer with DbContext, repositories, and database migrations for purchase transactions
- `infra-rates-exchange`: External API integration layer (Refit) for Treasury Reporting Rates of Exchange API

### Modified Capabilities

<!-- No existing capabilities have requirement-level changes; this is a structural refactoring -->

## Impact

- **Projects affected**: Solution structure, API layer (updates project references)
- **Dependencies**: EF Core, Refit, PostgreSQL
- **Code organization**: Clear separation of concerns between database and external APIs
- **Build artifacts**: Two new infrastructure packages instead of one monolithic package
