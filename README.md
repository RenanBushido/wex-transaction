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

### Health Check Endpoints

- **GET /health** — Liveness probe (Kubernetes `livenessProbe`): returns 200 if the process is running, regardless of dependency availability
- **GET /health/ready** — Readiness probe (Kubernetes `readinessProbe`): returns 200 if PostgreSQL and Treasury API are reachable; 503 otherwise

Both endpoints return a structured JSON response with individual check status:
```json
{
  "status": "Healthy",
  "entries": {
    "postgresql": { "status": "Healthy", "duration": "00:00:00.012" },
    "treasury-api": { "status": "Healthy", "duration": "00:00:00.245" }
  }
}
```

### Rate Limiting

Rate limiting is enabled on all transaction endpoints to prevent abuse and ensure API stability. The API uses ASP.NET Core's built-in sliding window rate limiter with per-endpoint policies:

**Default Limits (Production)**:
- **POST /api/v1/transaction**: 10 requests per 1 minute
- **GET /api/v1/transaction/{id}/location/{country}-{currency}**: 30 requests per 1 minute

**Development Limits** (configured in `appsettings.Development.json`):
- **POST /api/v1/transaction**: 100 requests per 1 minute
- **GET /api/v1/transaction/{id}/location/{country}-{currency}**: 300 requests per 1 minute

**Rate Limit Headers**:
All responses include rate limit information:
- `X-RateLimit-Limit`: Maximum requests allowed in the current window
- `X-RateLimit-Remaining`: Requests remaining in the current window
- `X-RateLimit-Reset`: Unix timestamp when the window resets

**Rate Limit Exceeded (HTTP 429)**:
When a client exceeds the rate limit, the API returns:
```
HTTP 429 Too Many Requests
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1719446460

Rate limit exceeded. Too many requests.
```

**Customizing Rate Limits**:
To adjust rate limits, edit the `appsettings.json` file:
```json
{
  "RateLimiting": {
    "Policies": {
      "post-policy": {
        "PermitLimit": 10,
        "WindowSeconds": 60
      },
      "get-policy": {
        "PermitLimit": 30,
        "WindowSeconds": 60
      }
    }
  }
}
```

**Important Notes**:
- Rate limits are **global** (not per-IP or per-user) — suitable for single-instance deployments
- Rate limiting is applied **after** exception handling, so internal errors are still handled normally
- For distributed deployments, consider Redis-backed rate limiting in future versions

### Exception Handling

- **GlobalExceptionHandler**: Middleware for centralized exception handling
  - Maps domain exceptions to HTTP status codes (417 Expectation Failed)
  - Maps validation exceptions to 400 Bad Request
  - Maps unhandled exceptions to 500 Internal Server Error
  - Returns ProblemDetails response format
  - Logs all exceptions with full stack trace via Serilog

### Service Registration Pattern

Services are registered via extension methods in `WexTransaction.CrossCutting/AppDependencies/`:

| Extension | Responsibility |
|-----------|---------------|
| `PersistenceExtensions` | DbContext (EF Core + Dapper), repositories, Unit of Work |
| `ApplicationExtensions` | MediatR, AutoMapper, FluentValidation |
| `ExternalApiExtensions` | Refit client for Treasury API, Polly resilience policies |
| `ResiliencePoliciesExtensions` | Retry, Circuit Breaker, Timeout (Polly) |
| `CorsExtensions` | CORS policy per environment |
| `LoggingExtensions` | Serilog two-stage initialization |
| `HealthCheckExtensions` | PostgreSQL and Treasury API health checks |
| `RateLimitingExtensions` | Per-endpoint rate limiting with sliding window algorithm |

## Infrastructure - Resilience, Observability & External APIs

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

### Structured Logging (Serilog)

- **Two-stage initialization**: Bootstrap logger captures startup errors before host is built
- **ReadFrom.Configuration()**: Log levels and sinks configured via `appsettings.json` / `appsettings.Development.json`
- **UseSerilogRequestLogging()**: One log event per HTTP request (replaces verbose ASP.NET Core default logs)
- **Structured templates**: All log calls use named parameters (e.g., `"Transaction {TransactionId} not found"`)
- **Environment-aware levels**: `Debug` in Development, `Information` in Production

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
- [ ] Correlation IDs for distributed tracing
- [ ] Event-driven architecture for audit trails (Event Sourcing)
- [ ] Caching layer (Redis) for exchange rates
- [ ] API versioning strategy
- [ ] Kubernetes manifest with `livenessProbe` and `readinessProbe` wired to `/health` and `/health/ready`
---

## Deployment & Infrastructure

This section provides step-by-step instructions for deploying the WEX Transaction API using Docker, docker-compose, and GitHub Actions CI/CD pipeline.

### Prerequisites

Before deploying, ensure you have the following tools installed:

- **Docker** (20.10 or later) - [Install Docker](https://docs.docker.com/install/)
- **Docker Compose** (2.0 or later) - [Install Docker Compose](https://docs.docker.com/compose/install/)
- **.NET 10 CLI** (optional, for local development) - [Install .NET](https://dotnet.microsoft.com/download)
- **PostgreSQL Client Tools** (optional, for manual database commands) - `psql` included in PostgreSQL
- **Git** (for cloning the repository)

### Quick Start - Local Development

Get the application running locally with docker-compose in 5 minutes:

```bash
# 1. Clone the repository
git clone https://github.com/renan79/wex-transaction.git
cd wex-transaction

# 2. Start the application stack (PostgreSQL + API)
docker-compose up -d

# 3. Wait for services to become healthy
docker-compose ps  # All services should show "healthy" or "Up"

# 4. Verify the API is responding
curl http://localhost:5000/health

# 5. Test with a sample transaction
curl -X POST http://localhost:5000/api/v1/transaction \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Test Transaction",
    "date": "2026-06-23T08:30:00Z",
    "amount": 100.50
  }'
```

The application will be available at `http://localhost:5000`.

### Docker Configuration

#### Dockerfile

The project uses a multi-stage Dockerfile for optimized image builds:

- **Build Stage**: Uses `mcr.microsoft.com/dotnet/sdk:10` to compile and publish the application
- **Runtime Stage**: Uses lightweight `mcr.microsoft.com/dotnet/aspnet:10` runtime-only image
- **Result**: Final image size under 400MB (vs. 1.2GB with single-stage build)

Key features:
- Restores NuGet packages
- Publishes application with Release configuration
- Exposes port 5000
- Includes health check endpoint
- Minimal attack surface with runtime-only image

#### docker-compose.yml

Production-ready orchestration of application and database services:

**Services:**
- `db`: PostgreSQL 15.18-alpine3.23 with persistent volume and health checks
- `app`: WEX Transaction API with health checks and resource limits

**Key Features:**
- Explicit bridge network for service communication
- Named volumes for database persistence
- Health checks for both services
- Memory limits: 512MB for app, 256MB for database
- Environment variable injection for configuration
- Service dependencies with health check conditions

**Health Checks:**
- Application: HTTP GET `/health` → expects 200 OK
- PostgreSQL: `pg_isready` command checks database connectivity

### Environment Configuration

#### Environment Variables

The application supports the following environment variables for configuration:

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Determines which appSettings file is loaded (Development or Production) |
| `ASPNETCORE_URLS` | `http://+:5000` | Server listen URLs |
| `ConnectionStrings__DefaultConnection` | See docker-compose.yml | Database connection string (EF Core) |
| `POSTGRES_USER` | `wexuser` | PostgreSQL database user |
| `POSTGRES_PASSWORD` | `wexpassword` | PostgreSQL database password |
| `POSTGRES_DB` | `wextransaction` | PostgreSQL database name |
| `RateLimiting__PostPolicy__PermitLimit` | `10` | POST endpoint requests per minute (production) |
| `RateLimiting__GetPolicy__PermitLimit` | `30` | GET endpoint requests per minute (production) |
| `DB_PORT` | `5432` | External PostgreSQL port mapping |
| `APP_PORT` | `5000` | External application port mapping |

#### appSettings Files

The application uses environment-specific configuration files:

**appSettings.json** (Production)
- Rate limiting: POST 10 req/min, GET 30 req/min
- Logging: Information level with Microsoft warnings
- Database: Connects to PostgreSQL service named `db`
- CORS: Restricted to configured origins
- Connection: Uses production credentials (defaults to `db` service)

**appSettings.Development.json** (Development/Staging)
- Rate limiting: POST 100 req/min, GET 300 req/min (relaxed for development)
- Logging: Debug level for detailed output
- Database: Connects to PostgreSQL service named `db`
- CORS: Allow all origins (suitable for development)
- Connection: Uses development credentials

#### .env File Configuration

Create a `.env` file in the project root to override default environment variables:

```bash
# Copy from example
cp .env.example .env

# Edit .env with your values
POSTGRES_USER=myuser
POSTGRES_PASSWORD=mysecurepassword
POSTGRES_DB=mydatabase
ASPNETCORE_ENVIRONMENT=Production
```

Load environment variables when starting docker-compose:
```bash
docker-compose up -d  # Automatically loads .env file
```

### Local Deployment

#### Using docker-compose (Recommended)

Start the complete application stack:

```bash
# Build and start all services in detached mode
docker-compose up -d

# Verify services are healthy
docker-compose ps

# View application logs
docker-compose logs -f app

# View database logs
docker-compose logs -f db
```

#### Using Docker CLI Directly

If you prefer manual Docker commands without docker-compose:

```bash
# Build the image
docker build -t wex-transaction:latest .

# Create PostgreSQL container
docker run -d \
  --name wex-postgres \
  -e POSTGRES_USER=wexuser \
  -e POSTGRES_PASSWORD=wexpassword \
  -e POSTGRES_DB=wextransaction \
  -p 5432:5432 \
  -v postgres_data:/var/lib/postgresql/data \
  postgres:15.18-alpine3.23

# Create and run application container (linked to database)
docker run -d \
  --name wex-api \
  -p 5000:5000 \
  -e ConnectionStrings__DefaultConnection="Host=wex-postgres;Port=5432;Database=wextransaction;Username=wexuser;Password=wexpassword;" \
  --link wex-postgres:wex-postgres \
  wex-transaction:latest
```

### Configuration Management

#### Modifying Rate Limits

**In docker-compose.yml:**
```yaml
environment:
  RateLimiting__PostPolicy__PermitLimit: 20  # Change POST limit to 20 req/min
  RateLimiting__GetPolicy__PermitLimit: 50   # Change GET limit to 50 req/min
```

**Via environment variables:**
```bash
export RateLimiting__PostPolicy__PermitLimit=20
docker-compose up -d
```

**In .env file:**
```
RateLimiting__PostPolicy__PermitLimit=20
RateLimiting__GetPolicy__PermitLimit=50
```

#### Modifying Database Connection

The default connection uses the `db` service name (docker-compose internal DNS). To connect to external PostgreSQL:

```bash
export ConnectionStrings__DefaultConnection="Host=external-server.com;Port=5432;Database=wextransaction;Username=user;Password=pass;"
docker-compose up -d
```

#### Modifying Logging Level

**In appSettings.json:**
```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Debug"  // Change from "Information" to "Debug" for detailed logs
  }
}
```

Then rebuild the Docker image:
```bash
docker-compose build --no-cache && docker-compose up -d
```

### Staging Deployment

Automatic deployment to staging environment on successful PR merge:

**How it works:**
1. Push changes to a feature branch
2. Create pull request to `main` or `master`
3. GitHub Actions runs automated build and tests
4. On successful merge, workflow automatically deploys to staging
5. Staging environment health check verifies deployment success

**Accessing Staging:**
- URL: Available via `STAGING_URL` secret in GitHub repository
- Health endpoint: `{STAGING_URL}/health`
- Example: `https://staging.example.com/health`

**Configuring Staging:**
- Set GitHub Secrets: `STAGING_URL`, `STAGING_DB_PASSWORD`, `STAGING_CONNECTION_STRING`
- Workflow file: `.github/workflows/ci-cd.yml` (deploy-staging job)

**Verify Staging Deployment:**
```bash
# Check GitHub Actions logs
# Navigate to Actions tab → "CI/CD Pipeline" → Latest run → "Deploy to Staging" job

# Manual verification
curl https://{STAGING_URL}/health
```

### Production Deployment

Manual deployment to production with safety gates:

**How it works:**
1. Manual trigger via GitHub Actions UI (or CLI)
2. Requires production environment approval (if configured)
3. Deploysapplication using production appSettings and secrets
4. Performs health check before marking deployment as successful
5. Logs deployment activity in GitHub Actions

**Triggering Production Deployment:**

Via GitHub UI:
1. Navigate to Actions tab
2. Select "CI/CD Pipeline" workflow
3. Click "Run workflow" dropdown
4. Select `deploy_env: production`
5. Click "Run workflow"

Via GitHub CLI:
```bash
gh workflow run ci-cd.yml -f deploy_env=production
```

**Configuring Production:**
- Set GitHub Secrets: `PROD_URL`, `PROD_DB_PASSWORD`, `PROD_CONNECTION_STRING`
- Workflow file: `.github/workflows/ci-cd.yml` (deploy-production job)

**Verify Production Deployment:**
```bash
# Check GitHub Actions logs for deployment status
gh run list --workflow=ci-cd.yml --status=completed

# Manual verification
curl https://{PROD_URL}/health
```

**Rollback Procedure:**
```bash
# Redeploy previous image tag
# Example: docker run wex-transaction:main-{previous-sha}

# Or redeploy from specific commit
gh workflow run ci-cd.yml -f deploy_env=production
# (Provide commit SHA if needed)
```

### CI/CD Pipeline

GitHub Actions workflow automates testing and deployment:

**Workflow File:** `.github/workflows/ci-cd.yml`

**Stages:**

1. **Build & Test** (runs on push and PR)
   - Checkout code
   - Setup .NET 10
   - Restore NuGet packages
   - Build solution (Release configuration)
   - Run test suite
   - Publish test results to GitHub

2. **Build Docker Image** (runs on push/workflow_dispatch)
   - Build multi-stage Docker image
   - Tag with commit SHA and `latest`
   - Push to GitHub Container Registry (ghcr.io)
   - Cache layers for faster subsequent builds

3. **Deploy Staging** (runs on merge to main)
   - Deploy to staging environment
   - Run health checks
   - Verify deployment success

4. **Deploy Production** (manual trigger only)
   - Deploy to production environment
   - Run health checks
   - Verify deployment success
   - Send notifications

**Required GitHub Secrets:**
```
STAGING_URL                    # Staging environment URL
STAGING_DB_PASSWORD            # Staging database password
STAGING_CONNECTION_STRING      # Staging database connection string

PROD_URL                       # Production environment URL
PROD_DB_PASSWORD               # Production database password
PROD_CONNECTION_STRING         # Production database connection string
```

**Viewing Workflow Status:**
```bash
# List recent workflow runs
gh run list --workflow=ci-cd.yml

# View specific run details
gh run view {run-id}

# Stream run logs
gh run view {run-id} --log
```

### Troubleshooting

#### Database Connection Failures

**Symptom:** Application logs show "Connection refused" or timeout errors

**Solution:**
```bash
# Check PostgreSQL service is running and healthy
docker-compose ps db
# Expected: "healthy" status

# Check database logs
docker-compose logs db

# Test connection manually
docker exec wex-postgres psql -U wexuser -d wextransaction -c "SELECT version();"

# Verify connection string in docker-compose.yml or .env
cat docker-compose.yml | grep ConnectionString
```

#### Port Conflicts

**Symptom:** "Address already in use" or similar when starting containers

**Solution:**
```bash
# Check what's using port 5000 or 5432
lsof -i :5000    # Application port
lsof -i :5432    # Database port

# Change ports in docker-compose.yml or .env
export APP_PORT=5001
export DB_PORT=5433
docker-compose up -d

# Or stop conflicting service
docker ps | grep wex-api
docker stop {container-id}
```

#### Health Check Failures

**Symptom:** "Health check failure" or "Unhealthy" container status

**Application health check failing:**
```bash
# Check application is listening on port 5000
docker exec wex-api curl -f http://localhost:5000/health

# View application logs
docker-compose logs -f app --tail=50

# Verify database connection is working
docker-compose logs app | grep -i "database\|connection"
```

**PostgreSQL health check failing:**
```bash
# Test pg_isready directly
docker exec wex-postgres pg_isready -U wexuser -d wextransaction

# Check PostgreSQL is accepting connections
docker exec wex-postgres psql -U wexuser -d wextransaction -c "SELECT 1;"
```

#### Clean Rebuild

If experiencing persistent issues, perform a full clean rebuild:

```bash
# Stop all containers
docker-compose down

# Remove volumes (WARNING: deletes database data!)
docker-compose down -v

# Remove unused images/containers
docker system prune -f

# Rebuild and restart
docker-compose build --no-cache
docker-compose up -d

# Verify health
docker-compose ps
```

#### View Application Logs

```bash
# Real-time application logs
docker-compose logs -f app

# Last 50 lines
docker-compose logs app --tail=50

# Database logs
docker-compose logs db

# All services
docker-compose logs
```

### Quick Reference Commands

```bash
# Start all services
docker-compose up -d

# Stop all services (data persists)
docker-compose down

# View service status
docker-compose ps

# View logs
docker-compose logs -f app              # Application logs
docker-compose logs -f db               # Database logs

# Restart services
docker-compose restart                  # All services
docker-compose restart app              # Just application
docker-compose restart db               # Just database

# Execute commands in container
docker exec wex-api dotnet --version    # .NET version
docker exec wex-postgres psql -U wexuser -d wextransaction  # PostgreSQL CLI

# Build without cache
docker-compose build --no-cache

# View image size
docker images wex-transaction:latest

# Full clean rebuild
docker-compose down -v && docker-compose build --no-cache && docker-compose up -d
```

### Environment Comparison

| Aspect | Development | Staging | Production |
|--------|-------------|---------|------------|
| **appSettings File** | appSettings.Development.json | appSettings.Development.json | appSettings.json |
| **Logging Level** | Debug | Debug | Information |
| **Rate Limiting (POST)** | 100 req/min | 100 req/min | 10 req/min |
| **Rate Limiting (GET)** | 300 req/min | 300 req/min | 30 req/min |
| **CORS Policy** | Allow all origins | Allow all origins | Restricted |
| **Database** | Local docker container | Remote/Staging server | Production server |
| **Health Checks** | Enabled | Enabled | Enabled |
| **Deployment Method** | docker-compose locally | GitHub Actions | GitHub Actions (manual) |

### Performance Tuning

#### Database Connection Pooling

The application uses EF Core with connection pooling. Adjust pool size in appSettings:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=db;...Max Pool Size=20;"
}
```

#### Memory Limits

Resource limits in docker-compose:

```yaml
deploy:
  resources:
    limits:
      memory: 512M    # Application memory limit
      cpus: '1.0'     # CPU limit (optional)
```

#### Docker Image Caching

Dockerfile uses multi-stage build with proper layer ordering for cache efficiency:

```dockerfile
# Stable layer: system dependencies
RUN apt-get install...

# Stable layer: .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:10

# Semi-stable: project files and NuGet restore
COPY .csproj files
RUN dotnet restore

# Unstable: source code (changes frequently)
COPY src/
RUN dotnet build
```

This ordering ensures frequently-changing layers are at the end, maximizing cache hits.

### Next Steps

- [x] Deploy locally with docker-compose
- [ ] Configure GitHub Secrets for staging/production
- [ ] Set up CI/CD pipeline in GitHub Actions
- [ ] Deploy to staging environment
- [ ] Configure production environment
- [ ] Deploy to production
- [ ] Monitor application health and logs
- [ ] Set up additional monitoring (logs, metrics, alerts)

---
