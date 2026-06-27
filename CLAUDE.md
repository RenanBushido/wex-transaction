# WEX Challenge

## Project
It is an API with two endpoints that stores purchase transactions and offers a query function; this function converts the purchase amount—originally in US dollars—into the currency of the country provided as a parameter, based on the current exchange rate.

---

## Tecnologies

-   .NET 10
-   EF Core
-   Docker
-   Postgres 15.18-alpine3.23
-   xUnit
-   Polly
-   Refit

---

## Architecture

The application will be developed using .NET 10, using Clean Architecture, and Implement global using with single file in the root project folder and a Postgres database and everything will be deployed via Docker with docker-compose.yml



### Functions

-   POST Endpoint for persistence purchase transaction
-   GET Endpoint for retrieve purchase transaction with amount converted

---

### POST Endpoint
    
-   Example:
        *Request:
        -   POST: /api/v1/transaction
                BODY: {
                        "description": "something",
                        "date": "2026-06-23T08:30:00Z",
                        "amount": 200.24
                }

        *Response:
            StatusCode: 201 (created),
            { "transactionId": "e5f60f9c-31be-470f-d05fcf99c312" }


### GET Endpoint

-   Example:
        *Request:
        -   GET: /api/v1/transaction/{transactionId}/location/{country}-{currency}

        *Response:
            StatusCode: 200 (success),
            { 
                "transactionId": "e5f6f9c-31be-470f-d05fcf99c312",
                "description": "something",
                "date": "2026-06-23T08:30:00Z",
                "amount": 200.25,
                "tax_rate": 5.25,
                "converted_value": 1051.32
            }

### External Endpoint

-   https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange

---

## Pesistence

The application will use Postgres database with Entity FrameworkCore.

The database will store:
- Purchase Transaction

The database data will be stored in the project's root database/ folder and set up via docker-compose.yml

---

## Develpment Guidelines

-   Use async/await whenever applicable
-   Use native ASP.NET Core dependency injection
-   Avoid code duplicate
-   Follow SOLID principles where applicable
-   Create a .NET solution with a Clean Architecture design
-   Use EF Core as the persistence mechanism
-   Use PostgreSQL version 15.18-alpine3.23 (Docker) as the database
-   Prioritize code readability and maintainability
-   All the functionalities will have a unit test
-   Create .gitignore file
-   Create .editorconfig file

---

## Service Registration Pattern

All `IServiceCollection` extensions for middleware registration, service configuration, and dependency injection must follow a standardized pattern to ensure scalability, maintainability, and decoupling.

### Guidelines

-   **Single Responsibility**: Each extension file handles one specific concern (persistence, external APIs, authentication, logging, etc.)
-   **Location**: All extensions must reside in the `Extensions/` folder within each project
-   **Naming Convention**: `{Responsibility}Extensions.cs` (e.g., `PersistenceExtensions.cs`, `ExternalApiExtensions.cs`)
-   **Fluent Interface**: All extension methods return `IServiceCollection` to enable method chaining
-   **Configuration**: Pass `IConfiguration` parameter when extensions need to read configuration values
-   **Decoupling**: Each extension is independent; removing or replacing one extension doesn't affect others

### Extension Types

| Extension | Purpose | Example |
|-----------|---------|---------|
| `DependencyInjectionExtensions.cs` | Application layer services, use cases, handlers | Registers Application layer services |
| `PersistenceExtensions.cs` | EF Core, DbContext, repositories, unit of work | Registers DbContext, repositories |
| `ExternalApiExtensions.cs` | Refit clients, external API integrations | Registers Treasury API client |
| `MiddlewareExtensions.cs` | Middlewares, exception handlers, filters | Adds exception handler, logging middleware |
| `AuthenticationExtensions.cs` | Authentication, authorization, policies | Configures JWT, auth schemes |
| `LoggingExtensions.cs` | Logging configuration, Serilog setup | Configures logging providers |

### Example Implementation

**`Extensions/PersistenceExtensions.cs`**

```csharp
namespace WexTransaction.Api.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found.");
        
        services.AddDbContext<WexTransactionDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        
        return services;
    }
}
```

**`Extensions/ExternalApiExtensions.cs`**

```csharp
namespace WexTransaction.Api.Extensions;

public static class ExternalApiExtensions
{
    public static IServiceCollection AddExternalApis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRefitClient<ITreasuryExchangeRateClient>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://fiscaldata.treasury.gov");
                c.Timeout = TimeSpan.FromSeconds(30);
            });
        
        services.AddScoped<IExchangeRateProvider, TreasuryExchangeRateProvider>();
        
        return services;
    }
}
```

**`Program.cs`**

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Register all extensions in dependency order
builder.Services
    .AddPersistence(builder.Configuration)
    .AddExternalApis(builder.Configuration)
    .AddDependencyInjection()
    .AddLogging(builder.Configuration);

var app = builder.Build();

app
    .UseMiddlewares()
    .MapEndpoints();

app.Run();
```

---

## Fisical structure

All the source code must be created inside the src/ folder in the project's root, and the database data database/ folder and unit tests in tests/ folder

Structure desired:
database/
src/
|__WexTransaction
    |__WexTransaction.slnx
    |__WexTransaction/
tests/

Using solution extension .slnx for solution file