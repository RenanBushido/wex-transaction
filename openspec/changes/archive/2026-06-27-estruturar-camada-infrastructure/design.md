## Context

The current `WexTransaction.Infrastructure` project is a stub with only Class1.cs. This design defines how to restructure it into two focused projects:

1. **WexTransaction.Infra.Database** — EF Core persistence layer (DbContext, repositories, migrations)
2. **WexTransaction.Infra.Services.RatesExchange** — Refit HTTP client for Treasury API integration

This separation follows the Single Responsibility Principle and the Clean Architecture pattern. Each project can evolve independently, dependencies are explicit, and testing is simplified.

## Goals / Non-Goals

**Goals:**
- Separate database concerns from external API integrations
- Establish a clear, modular structure for future infrastructure additions
- Follow the Extension pattern for dependency registration
- Enable independent scaling and replacement of infrastructure components
- Maintain backward compatibility with the API layer

**Non-Goals:**
- Replace EF Core or Refit with alternative libraries (not in scope here)
- Implement advanced caching strategies (basic caching only in RatesExchange)
- Add API authentication beyond what Treasury API requires
- Create abstraction layers beyond Repository and ExchangeRateProvider

## Decisions

### 1. Two Projects vs. Monolithic Infrastructure

**Decision:** Split into `WexTransaction.Infra.Database` and `WexTransaction.Infra.Services.RatesExchange`.

**Rationale:** 
- **Database is stable**: Rarely changes once migration strategy is established. Deserves its own project boundary.
- **External APIs evolve**: Treasury API may change, or new external services may be added. Decoupling allows independent updates.
- **Clearer dependencies**: API layer knows exactly what it depends on (database, rates service).
- **Testing**: Easier to test database logic without Refit complexity, and vice versa.

**Alternatives considered:**
- Single Infrastructure project: Simpler initially, but couples unrelated concerns and reduces modularity.
- Separate projects per feature: Overkill for current scope; splitting into two is the minimum viable separation.

### 2. Project Naming Convention

**Decision:** Use `WexTransaction.Infra.{Concern}` naming (e.g., `WexTransaction.Infra.Database`, `WexTransaction.Infra.Services.RatesExchange`).

**Rationale:**
- **Infra prefix**: Signals these are infrastructure layer projects, grouped together in solution explorer.
- **Descriptive suffix**: Clarifies the specific responsibility (Database, Services.RatesExchange).
- **Scalable**: Future additions (e.g., `WexTransaction.Infra.Services.EmailNotification`) follow the same pattern.

**Alternatives considered:**
- `WexTransaction.Infrastructure.{Concern}`: More explicit but verbose and nests deep in solution.
- `WexTransaction.{Concern}Infra`: Less clear that these are infrastructure-layer projects.

### 3. Repository Pattern in Database Project

**Decision:** Implement generic repository interfaces and concrete repositories for PurchaseTransaction in `WexTransaction.Infra.Database`.

**Rationale:**
- Clean separation of data access logic from domain.
- Supports multiple implementations (e.g., in-memory for testing, EF Core for production).
- Abstracts query complexity; Application layer focuses on business logic.

**Alternatives considered:**
- Direct DbContext usage in Application: Couples Application to infrastructure details.
- Entity Framework queries directly in use cases: Violates separation of concerns.

### 4. EF Core Migrations Management

**Decision:** Migrations are created and stored in `WexTransaction.Infra.Database` project. Auto-migration on startup for development; manual migration in production.

**Rationale:**
- **Development**: Auto-migration accelerates feedback loop and keeps schema in sync.
- **Production**: Manual migration allows rollback planning and audit trails.

**Alternatives considered:**
- Always auto-migrate: Risk of unplanned schema changes in production.
- No auto-migration anywhere: Requires manual migration steps; friction in development.

### 5. External API Client Architecture

**Decision:** Use Refit interface + dependency injection to abstract HTTP calls. Define `IExchangeRateProvider` interface in Application layer (Services folder). Implement in `WexTransaction.Infra.Services.RatesExchange`.

**Rationale:**
- **Refit simplicity**: Declarative HTTP client, reduces boilerplate.
- **Interface abstraction**: Application layer defines the contract (`IExchangeRateProvider` in Services), Infrastructure provides implementation.
- **Testability**: Mock `IExchangeRateProvider` in tests without mocking HTTP.
- **Clean boundaries**: Application never depends on `Infra.Services.RatesExchange`; only on the interface it defines.

**Alternatives considered:**
- HttpClient directly: Verbose, couples Application to HTTP concerns.
- Manual REST calls: Unnecessary boilerplate; Refit handles this.

### 6. Caching Strategy for Exchange Rates

**Decision:** Implement in-memory caching in `ExchangeRateProvider` with configurable TTL (default 1 hour) to reduce API calls.

**Rationale:**
- Exchange rates change infrequently; caching reduces Treasury API load and latency.
- Application layer is not aware of caching; transparent optimization.

**Alternatives considered:**
- No caching: Excessive API calls for identical requests.
- Distributed cache (Redis): Overkill for current scale; adds operational complexity.

### 7. Extension Registration Pattern

**Decision:** Create `PersistenceExtensions.cs` and `ExternalApiExtensions.cs` in each project's `Extensions/` folder. Register all services in Program.cs via fluent chaining.

**Rationale:**
- Follows standardized extension pattern (established in CLAUDE.md).
- Each project owns its registration logic; changes to one project don't affect others' setup.
- Configuration reading is localized (e.g., `PersistenceExtensions` reads connection string; `ExternalApiExtensions` reads API URL).

**Alternatives considered:**
- Central registration in API project: Couples API to infrastructure details.
- Service Locator pattern: Violates DI best practices.

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| **Circular dependency**: RatesExchange references Database | Ensure RatesExchange has NO dependency on Database. Both depend only on Domain. Code review before merging. |
| **Breaking changes in API layer**: When API references new projects | Updated API project file to reference both new projects. Run `dotnet build` to verify. |
| **Migration complexity**: Existing data must be migrated if schema changes | Use EF Core migration tooling; auto-migration handles schema alignment. Test migrations in staging environment first. |
| **Configuration duplication**: Multiple projects read from IConfiguration | Acceptable; each project is responsible for its own config. Alternative (centralized config) would reduce modularity. |
| **API unavailability**: Treasury API goes down during rate fetch | IExchangeRateProvider throws exception; handled by Application/API layer. Fallback caching mitigates partial outages. |

## Migration Plan

1. **Create new projects** in folder structure:
   - `src/WexTransaction/WexTransaction.Infra.Database/`
   - `src/WexTransaction/WexTransaction.Infra.Services.RatesExchange/`

2. **Remove old project** and Class1.cs:
   - Delete `src/WexTransaction/WexTransaction.Infrastructure/`

3. **Implement Database project**:
   - Create `DbContext` for PurchaseTransaction
   - Implement `ITransactionRepository`
   - Add initial migration
   - Create `PersistenceExtensions.cs`

4. **Implement RatesExchange project**:
   - Create Refit interface for Treasury API
   - Implement `TreasuryExchangeRateProvider` (implements `IExchangeRateProvider`)
   - Add caching layer
   - Create `ExternalApiExtensions.cs`

5. **Implement Application Services**:
   - Define `IExchangeRateProvider` interface in Application/Services

6. **Update solution file**:
   - Add both new projects to .slnx

7. **Update API project**:
   - Update project references
   - Update Program.cs to call extension methods
   - Verify build: `dotnet build src/WexTransaction/WexTransaction.slnx`

8. **Verify**:
   - All tests pass: `dotnet test`
   - No compiler warnings

## Open Questions

- What is the TTL for exchange rate cache? 1 hour seems reasonable but should be configurable.
  - **Decision pending**: Propose 1 hour default, override via IConfiguration.
- Should there be a `WexTransaction.Infra.Common` project for shared infrastructure abstractions (e.g., base repository class)?
  - **Decision pending**: Not in current scope; add later if needed.
