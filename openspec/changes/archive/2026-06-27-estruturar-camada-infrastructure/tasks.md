## 1. Project Structure Setup

- [x] 1.1 Delete `WexTransaction.Infrastructure` project and Class1.cs
- [x] 1.2 Create folder `src/WexTransaction/WexTransaction.Infra.Database/`
- [x] 1.3 Create folder `src/WexTransaction/WexTransaction.Infra.Services.RatesExchange/`
- [x] 1.4 Create `WexTransaction.Infra.Database.csproj` with .NET 10 SDK, EF Core, Npgsql references
- [x] 1.5 Create `WexTransaction.Infra.Services.RatesExchange.csproj` with .NET 10 SDK, Refit references
- [x] 1.6 Update solution file `WexTransaction.slnx` to include both new projects and remove old Infrastructure project

## 2. WexTransaction.Infra.Database - Foundation

- [x] 2.1 Create `WexTransaction.Infra.Database/GlobalUsings.cs` with project-namespace global usings
- [x] 2.2 Create `WexTransaction.Infra.Database/WexTransactionDbContext.cs` class
- [x] 2.3 Configure DbContext to use PostgreSQL and map PurchaseTransaction entity
- [x] 2.4 Create `.editorconfig` file in project root (if not inherited from solution)
- [x] 2.5 Add project references: Domain layer (`WexTransaction.Domain`)

## 3. WexTransaction.Infra.Database - Repositories

- [x] 3.1 Create `WexTransaction.Infra.Database/Repositories/ITransactionRepository.cs` interface
- [x] 3.2 Implement `GetByIdAsync(Guid id)` method in interface
- [x] 3.3 Implement `AddAsync(PurchaseTransaction transaction)` method in interface
- [x] 3.4 Create `WexTransaction.Infra.Database/Repositories/TransactionRepository.cs` implementation
- [x] 3.5 Implement all repository methods using DbContext
- [x] 3.6 Add unit tests for repository (in `tests/WexTransaction.Tests/Infrastructure/Repositories/`)

## 4. WexTransaction.Infra.Database - Migrations

- [x] 4.1 Create initial EF Core migration: `dotnet ef migrations add InitialCreate --project WexTransaction.Infra.Database`
- [x] 4.2 Verify migration creates `purchase_transactions` table with correct columns (id, description, transaction_date, amount)
- [x] 4.3 Create `WexTransaction.Infra.Database/Extensions/PersistenceExtensions.cs` for DI registration
- [x] 4.4 In `PersistenceExtensions.AddPersistence()`, register DbContext with connection string from IConfiguration
- [x] 4.5 In `PersistenceExtensions.AddPersistence()`, register ITransactionRepository → TransactionRepository
- [x] 4.6 Add method to DbContext context to auto-apply migrations on startup (optional, development-friendly)

## 5. WexTransaction.Infra.Services.RatesExchange - Foundation

- [x] 5.1 Create `WexTransaction.Infra.Services.RatesExchange/GlobalUsings.cs` with project-namespace global usings
- [x] 5.2 Create `.editorconfig` file in project root (if not inherited from solution)
- [x] 5.3 Add project references: Domain layer (`WexTransaction.Domain`)
- [x] 5.4 Add NuGet reference: `Refit` (latest stable)
- [x] 5.5 Add NuGet reference: `Polly` (for resilience policies, if not already in csproj)

## 6. WexTransaction.Application - Services Contracts

- [x] 6.1 Create `WexTransaction.Application/Services/IExchangeRateProvider.cs` interface
- [x] 6.2 Define `GetExchangeRatesAsync(string country, string currency)` method returning IEnumerable<ExchangeRate>

## 7. WexTransaction.Infra.Services.RatesExchange - Refit Client

- [x] 7.1 Create `WexTransaction.Infra.Services.RatesExchange/Clients/ITreasuryExchangeRateClient.cs` Refit interface
- [x] 7.2 Define Refit method to fetch exchange rates by country and currency from Treasury API
- [x] 7.3 Create `WexTransaction.Infra.Services.RatesExchange/Models/TreasuryRateDto.cs` DTO for API response
- [x] 7.4 Map TreasuryRateDto fields to ExchangeRate domain value object

## 8. WexTransaction.Infra.Services.RatesExchange - Provider Implementation

- [x] 8.1 Create `WexTransaction.Infra.Services.RatesExchange/Providers/TreasuryExchangeRateProvider.cs` implementing IExchangeRateProvider
- [x] 8.2 Implement `GetExchangeRatesAsync(string country, string currency)` method using Refit client
- [x] 8.3 Implement caching layer in provider (in-memory cache, 1-hour TTL by default)
- [x] 8.4 Implement error handling: catch HTTP exceptions and throw CurrencyConversionUnavailableException
- [x] 8.5 Add unit tests for provider (in `tests/WexTransaction.Tests/Infrastructure/Providers/`)
- [x] 8.6 Add unit tests for Refit client integration with mocked HTTP responses

## 9. WexTransaction.Infra.Services.RatesExchange - Extensions

- [x] 9.1 Create `WexTransaction.Infra.Services.RatesExchange/Extensions/ExternalApiExtensions.cs`
- [x] 9.2 In `ExternalApiExtensions.AddExternalApis()`, register Refit client with base URL from IConfiguration
- [x] 9.3 In `ExternalApiExtensions.AddExternalApis()`, set HTTP client timeout (default 30 seconds)
- [x] 9.4 In `ExternalApiExtensions.AddExternalApis()`, register IExchangeRateProvider → TreasuryExchangeRateProvider
- [x] 9.5 Add Polly resilience policy (retry, circuit breaker) to HTTP client (optional enhancement)

## 10. API Layer Integration

- [x] 10.1 Update `WexTransaction.Api.csproj` project references: remove old Infrastructure, add Database and RatesExchange
- [x] 10.2 Update `WexTransaction.Api/GlobalUsings.cs` to include new infrastructure namespaces if needed
- [x] 10.3 Update `Program.cs`: Call `builder.Services.AddPersistence(builder.Configuration)` before other registrations
- [x] 10.4 Update `Program.cs`: Call `builder.Services.AddExternalApis(builder.Configuration)` for rates exchange
- [x] 10.5 Verify Application layer can inject ITransactionRepository and IExchangeRateProvider

## 11. Configuration & Build Verification

- [x] 11.1 Add `DefaultConnection` string to `appsettings.json` or `appsettings.Development.json`
- [x] 11.2 Add Treasury API base URL to configuration (e.g., `TreasuryApiUrl` in `appsettings.json`)
- [x] 11.3 Build entire solution: `dotnet build src/WexTransaction/WexTransaction.slnx` → expect 0 errors, 0 warnings
- [x] 11.4 Run all tests: `dotnet test` → expect all existing tests + new infrastructure tests to pass
- [x] 11.5 Verify application startup with: `dotnet run --project src/WexTransaction/WexTransaction.Api/WexTransaction.Api.csproj`

## 12. Documentation & Cleanup

- [x] 12.1 Add README.md to `WexTransaction.Infra.Database/` explaining DbContext and migrations
- [x] 12.2 Add README.md to `WexTransaction.Infra.Services.RatesExchange/` explaining Treasury API integration
- [x] 12.3 Update root CLAUDE.md with final project structure diagram
- [x] 12.4 Verify no stale `bin/` or `obj/` folders in old Infrastructure project
- [x] 12.5 Commit all changes: "Restructure Infrastructure layer into Database and RatesExchange projects"
