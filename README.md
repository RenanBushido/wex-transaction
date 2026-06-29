# WexTransaction

# Requirements

## Requirement #1: Store a Purchase Transaction

Your application must be able to accept and store (i.e., persist) a purchase transaction with a description, transaction
date, and a purchase amount in **United States dollars**. When the transaction is stored, it will be assigned a **unique identifier**.

## Field requirements

● Description: must not exceed 50 characters
● Transaction date: must be a valid date format
● Purchase amount: must be a valid positive amount rounded to the nearest cent
● Unique identifier: must uniquely identify the purchase

## Requirement #2: Retrieve a Purchase Transaction in a Specified Country’s Currency

Provide a way to retrieve the stored purchase transactions converted to currencies supported by the **Treasury Reporting Rates of Exchange API**
based upon the exchange rate active for the date of the purchase.

**https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange**

The retrieved purchase should include the identifier, the description, the transaction date, the original US dollar purchase
amount, the exchange rate used, and the converted amount based upon the specified currency’s exchange rate for the
date of the purchase.

## Currency conversion requirements

● When converting between currencies, you do not need an exact date match, but must use a currency conversion rate 
    less than or equal to the purchase date from within the last 6 months.
● If no currency conversion rate is available within 6 months equal to or before the purchase date, an error should be 
    returned stating the purchase cannot be converted to the target currency.
● The converted purchase amount to the target currency should be rounded to two decimal places (i.e., cent).

## General Structure

- [x] The project will use **OpenSpec** as its AI tool.
- [x] The project will use Clean Architecture as its design architecture.
- [x] The project will be divided into layers, adhering to the architectural design.
- [x] There is no information regarding the volume of requests, but the application will be prepared.
- [x] Build the **Domain** layer.
- [x] Build the **Application** layer.
- [x] Build the **Infrastructure** layer.
- [x] Build the **Presentation** layer.


## OpenSpec

-   I used OpenSpec to help me create this application. I employed the Spec-Driven Development methodology to build the application, refining each step of the project structure whenever possible.
-   After that, I chose Clean Architecture and the SOLID principles as the design architecture.
-   Also i created a **SKILL** to review each step that I advanced

## Domain Layer

-   I used some conecepts of DDD, creating IEntity, ValueObjects and IAuditablesEntity for shared the same identity.
-   Simplify extensibility and enable polymorphism and follow the principles Open/Close of SOLID. 
-   ExchangeRateSelector it's Domain Service (DDD) application orquestration.

## Infrastructure Layer

-   This layer has 2 projects (Database and ServiceRatesExchange)
-   **Database**: 
    -   Implemented the two types of Repositories (ITransactionRepository and ITransactionDapperRepository Interfaces). I choose EF Core for persistence (change state) and Dapper for retrieve the informations, preparing the application to avoid concurrency of events like: Queries and Persistences.
    -   I used EF Core as the persistence tool.
    -   I used Dapper for Queries
    -   I prepared the code for execute Migrations.

-   **Services RatesExchange**:
    -   I used **Refit** e **HttpClient Factory** as tool to call an external API.

## Application Layer

-   This layer consist in implementation with Mediatr, using the approach CQRS.
-   Implements Command-Query Responsibility Segregation (CQRS) pattern with MediatR
-   Includes MediatR Pipeline Behaviors for cross-cutting concerns (validation, exception handling)
-   Uses FluentValidation for request validation integrated with MediatR pipeline
-   Includes AutoMapper MappingProfile for Entity-to-DTO transformations
-   Separates read (Query) and write (Command) operations for scalability and testability

### CQRS Handlers

- **SaveTransactionCommandHandler**: Handles purchase transaction creation with validation
- **GetPurchaseTransactionRequestHandler**: Handles transaction retrieval with currency conversion

### Behaviors

- **ValidationBehavior**: Validates requests using FluentValidation before handlers execute
- **UnhandledExceptionBehaviour**: Catches and transforms domain exceptions to application responses

## API Layer (Presentation)

### Minimal API Endpoints

- **POST /api/v1/transaction**: Save a new purchase transaction
  - Request: `{ "description": "string (max 50)", "date": "ISO8601", "amount": "decimal" }`
  - Response (201): `{ "transactionId": "guid" }`
  - Response (400): Validation errors

- **GET /api/v1/transaction/{id}/location/{country}-{currency}**: Retrieve transaction with currency conversion
  - Response (200): 
    ```json
    {
      "transactionId": "guid",
      "description": "string",
      "date": "ISO8601",
      "amount": "decimal",
      "tax_rate": "decimal",
      "converted_value": "decimal"
    }
    ```
  - Response (404): Transaction not found
  - Response (400): Invalid currency/country or conversion unavailable

### Exception Handling

- **GlobalExceptionHandler**: Middleware for centralized exception handling
  - Maps domain exceptions to HTTP status codes (422 Unprocessable Entity)
  - Maps validation exceptions to 400 Bad Request
  - Maps unhandled exceptions to 500 Internal Server Error
  - Returns ProblemDetails response format

### Service Registration Pattern

Services are registered via extension methods in `WexTransaction.CrossCutting/AppDependencies/`:

- **PersistenceExtensions**: Registers DbContext, repositories, and Unit of Work
- **ApplicationExtensions**: Registers MediatR, AutoMapper, and validators
- **ExternalApiExtensions**: Registers Refit client for Treasury API

## Infrastructure - Resilience & External APIs

### Polly Resilience Policies

The application includes production-grade resilience policies for external API calls:

- **Retry Policy**: Exponential backoff (3 attempts, 1s-4s delays)
- **Circuit Breaker**: Opens after 5 failures, resets after 30s
- **Timeout Policy**: 10-second request timeout with optimistic strategy
- **Combined PolicyHttpMessageHandler**: Integrates all policies seamlessly

These policies wrap the `ITreasuryExchangeRateClient` (Refit) to ensure:
- Graceful handling of temporary API failures
- Prevention of cascading failures
- Automatic recovery detection
- Request timeout protection

### External API Integration

- **Refit Client**: Type-safe HTTP client for Treasury Reporting Rates API
- **HttpClientFactory**: Built-in factory pattern for HTTP client management
- **Configuration**: BaseAddress and timeout configured via appsettings.json

## AutoMapper Integration

### MappingProfile

Centralized mapping configuration in `WexTransaction.Application/Mappings/MappingProfile.cs`:

- Maps `PurchaseTransaction` (domain entity) to `GetPurchaseTransactionResponse` (DTO)
- Handles value object conversions via implicit operators
  - `TransactionDescription` → `string`
  - `Money` → `decimal`
- Ignores computed properties (`TaxRate`, `ConvertedValue`) populated by handlers

**Key Features**:
- Automapper scans assemblies for MappingProfile registration
- Eliminates boilerplate mapping code
- Centralizes transformation logic
- Maintains clean separation between layers

Domain Layer - Estrutura & Organização:
- ✅ GlobalUsings.cs criado com imports de projeto (segue CLAUDE.md)
- ✅ Estrutura de pastas bem organizada:
  - Common/ — Interfaces base (IEntity, IEntityAuditable) ✓
  - Entities/ — Agregados raiz (PurchaseTransaction) ✓
  - ValueObjects/ — Valores imutáveis (Money, TransactionDescription, ExchangeRate, ConvertedTransactionResult) ✓
  - Exceptions/ — Exceções de domínio customizadas ✓
  - Services/ — Serviços de domínio puros (ExchangeRateSelector) ✓
  - Interfaces/ — Contratos adicionais ✓

Database Layer - Estrutura & Implementação:
- ✅ GlobalUsings.cs com imports apropriados (Microsoft.EntityFrameworkCore, ValueConversion, etc) ✓
- ✅ Estrutura modular:
  - Extensions/ — PersistenceExtensions (segue padrão CLAUDE.md) ✓
  - Repositories/ — ITransactionRepository, TransactionRepository com padrão genérico ✓
  - Config/ — Configuração de entidades EF Core ✓
  - Data/ — DbContext (WexTransactionDbContext) ✓
  - Migrations/ — Versionamento EF Core ✓

Aderência a CLAUDE.md:
- ✅ Extension Pattern: PersistenceExtensions em Extensions/ folder ✓
- ✅ Fluent Interface: AddPersistence retorna IServiceCollection ✓
- ✅ Async/Await: Todos os métodos são async (Task<T>) ✓
- ✅ Single Responsibility: Cada classe tem responsabilidade única ✓
- ✅ SOLID Principles:
  - S: Repository tem única responsabilidade (CRUD) ✓
  - O: Aberto para extensão (Generic Repository pattern) ✓
  - L: Implementações substituem interfaces corretamente ✓
  - I: Interfaces segregadas (ITransactionRepository, IEntity, IEntityAuditable) ✓
  - D: Depende de abstrações (ITransactionRepository, IConfiguration) ✓

Clean Architecture:
- ✅ Domain Layer: Não depende de nada (Domain é independente) ✓
- ✅ Database Layer: Depende apenas de Domain ✓
- ✅ Sem dependências circulares ✓
- ✅ ValueConverters para mapear value objects ao banco ✓

Base Repository Pattern (Novo):
- ✅ Classe genérica BaseRepository<T> para operações CRUD comuns ✓
- ✅ TransactionRepository herda de BaseRepository, adiciona lógica específica ✓
- ✅ Reduz duplicação de código ✓

UnitOfWork Pattern (Novo):
- ✅ IUnitOfWork interface para coordenar transações ✓
- ✅ UnitOfWork implementação com Commit() assíncrono ✓
- ✅ Centraliza SaveChanges() logic ✓

🔍 Boas Práticas Aplicadas

✅ Repository Pattern: Abstração clara de data access
✅ Unit of Work Pattern: Coordenação de transações
✅ Value Converters: Mapeamento elegante de value objects
✅ Generic Base: BaseRepository reduz código duplicado
✅ Null Checks: Validação apropriada em constructores
✅ Async All The Way: Sem sync-over-async antipattern
✅ Configuration-Driven: Connection strings em appsettings.json

## Testing Structure

### Test Layers

The test suite is organized to mirror the source code structure (`src/` → `tests/`):

```
tests/WexTransaction.Tests/
├── Domain/              (Entity, ValueObject, Exception tests)
├── Application/         (Handler, Behavior, Validator, Mapping tests)
├── Infrastructure/      (DbContext, Repository, Extension tests)
├── Api/                 (Endpoint, ExceptionHandler tests)
└── GlobalUsing.cs       (Test-wide imports and fixtures)
```

### Test Framework & Tools

- **xUnit**: Test framework for all unit and integration tests
- **Moq**: Mocking library for dependencies
- **FluentAssertions** (if added): Fluent assertion syntax
- **xUnit.Analyzers**: Code quality for xUnit tests
- **In-Memory DbContext**: For EF Core query tests without external database

### Test Categories

**Domain Layer Tests**:
- Entity creation and state validation
- Value object immutability and conversions
- Domain service logic (ExchangeRateSelector)
- Custom domain exceptions

**Application Layer Tests**:
- MediatR Command Handler execution and persistence
- MediatR Query Handler with in-memory database
- MediatR Pipeline Behaviors (validation, exception handling)
- FluentValidation Validators for request validation
- AutoMapper MappingProfile conversions

**Infrastructure Layer Tests**:
- EF Core DbContext configuration
- Entity Framework migrations and data modeling
- Repository CRUD operations
- Service registration extensions (DI)

**API Layer Tests**:
- Minimal API endpoint functionality
- Request validation and error responses
- GlobalExceptionHandler mapping and response format
- HTTP status codes and response types

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage report
dotnet test /p:CollectCoverage=true

# Run specific test category
dotnet test --filter Category=Domain
```

### Test Coverage Goals

- **Target Coverage**: 80%+ for core business logic
- **Priority Order**: Domain → Application → Infrastructure → API
- **Critical Paths**: Currency conversion, transaction persistence, error handling

## Development & Deployment

### Local Development

```bash
# Build solution
dotnet build

# Run API
cd src/WexTransaction/WexTransaction.Api
dotnet run

# Run tests
dotnet test
```

### Docker Deployment

```bash
# Build and run with docker-compose
docker-compose up --build

# Services:
# - API: http://localhost:5000 (with HTTPS redirection)
# - PostgreSQL: localhost:5432 (credentials in .env)
```

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName -p src/WexTransaction/WexTransaction.Infra.Database -s src/WexTransaction/WexTransaction.Api

# Apply migrations
dotnet ef database update -p src/WexTransaction/WexTransaction.Infra.Database -s src/WexTransaction/WexTransaction.Api
```

## Architecture Decisions

### CQRS with MediatR

- Separates read and write operations for independent scaling
- Enables parallel development of queries and commands
- Facilitates testing through pipeline behaviors
- Simplifies adding cross-cutting concerns (validation, logging)

### Hybrid EF Core + Dapper Approach

- **EF Core**: Used for write operations (persistence, transactions)
- **Dapper**: Optimized for read operations (performance)
- **Benefit**: Write consistency + read performance optimization

### Value Objects in Domain

- Encapsulates domain concepts (Money, TransactionDescription, ExchangeRate)
- Ensures type safety and prevents invalid states
- Simplified mappings to DTOs via implicit operators

### Resilience Policies (Polly)

- Protects against transient failures in external API calls
- Configurable retry, circuit breaker, and timeout strategies
- Prevents cascading failures in distributed systems

## Known Limitations & Future Improvements

- [ ] Authentication & Authorization (JWT, roles-based access)
- [ ] Rate limiting and API throttling
- [ ] Comprehensive API logging and correlation IDs
- [ ] Event-driven architecture for audit trails (Event Sourcing)
- [ ] Caching layer (Redis) for exchange rates
- [ ] API versioning strategy
- [ ] GraphQL support (alternative to REST)