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
-   AutoMapper (for Entity-DTO mapping)

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
| `DependencyInjectionExtensions.cs` | Application layer services, AutoMapper setup, use cases | Registers AutoMapper for Entity-DTO mapping |
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

**`Extensions/DependencyInjectionExtensions.cs` (Application Layer)**

```csharp
namespace WexTransaction.Application.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper with assembly scanning for MappingProfile
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        
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

## AutoMapper MappingProfile Pattern

The Application layer uses AutoMapper to handle Entity-to-DTO transformations automatically. This eliminates boilerplate mapping code and centralizes transformation logic.

### MappingProfile Structure

**Location**: `WexTransaction.Application/Mappings/MappingProfile.cs`

**Key Principles**:
- Inherits from `AutoMapper.Profile` class
- Defines mappings between Domain entities and Application DTOs
- Uses `CreateMap<Source, Destination>()` for configuration
- Handles value object conversions via implicit operators or `.ForMember()` configurations
- Registered automatically in DependencyInjectionExtensions via `typeof(MappingProfile).Assembly`

### Example: Entity-DTO Mapping

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map PurchaseTransaction entity to QueryTransactionResponse DTO
        CreateMap<PurchaseTransaction, QueryTransactionResponse>()
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => (string)src.Description)) // Value object conversion
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.TransactionDate))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => (decimal)src.Amount)) // Value object conversion
            .ForMember(dest => dest.TaxRate, opt => opt.Ignore()) // Populated by application logic
            .ForMember(dest => dest.ConvertedValue, opt => opt.Ignore()); // Populated by application logic
    }
}
```

### Value Object Conversions

Domain layer uses value objects (Money, TransactionDescription) with implicit operators for conversion:
- `TransactionDescription` has implicit `operator string` → maps directly via casting
- `Money` has implicit `operator decimal` → maps directly via casting
- These conversions maintain domain integrity while enabling clean DTOs

---

## CQRS Pattern (Phase 2A)

The Application layer implements Command Query Responsibility Segregation (CQRS) using MediatR to separate read and write operations, improving scalability and testability.

### Architecture Flow

```
API Controller (HTTP)
    ↓
MediatR.Send(Command/Query)
    ↓
Handler (IRequestHandler)
    ↓
Domain Layer (Business Logic)
    ↓
Infrastructure (Repository, External APIs)
```

### Command Pattern (Write Operations)

**CreateTransactionCommand** - Handles purchase transaction creation

```csharp
public record CreateTransactionCommand(
    string Description,
    DateTime Date,
    decimal Amount
) : IRequest<Guid>;
```

**CreateTransactionCommandHandler** - Processes the command

```csharp
public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = PurchaseTransaction.Create(request.Description, request.Date, request.Amount);
        await _repository.AddAsync(transaction);
        await _unitOfWork.Commit(cancellationToken);
        
        return transaction.Id;
    }
}
```

### Query Pattern (Read Operations)

**GetTransactionQuery** - Retrieves and converts transaction details

```csharp
public record GetTransactionQuery(
    Guid TransactionId,
    string Country,
    string Currency
) : IRequest<QueryTransactionResponse?>;
```

**GetTransactionQueryHandler** - Processes the query

```csharp
public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, QueryTransactionResponse?>
{
    private readonly ITransactionRepository _repository;
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly IMapper _mapper;
    
    public async Task<QueryTransactionResponse?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByIdAsync(request.TransactionId);
        if (transaction == null)
            return null;
        
        var exchangeRates = await _exchangeRateProvider.GetExchangeRatesAsync(request.Country, request.Currency);
        var selectedRate = ExchangeRateSelector.SelectRate((decimal)transaction.Amount, transaction.TransactionDate, exchangeRates);
        
        var response = _mapper.Map<QueryTransactionResponse>(transaction);
        response = response with
        {
            TaxRate = selectedRate.Rate,
            ConvertedValue = (decimal)transaction.Amount * selectedRate.Rate
        };
        
        return response;
    }
}
```

## MediatR Integration (Phase 2A)

MediatR serves as the central mediator for all command and query processing, decoupling the API layer from application logic.

### Service Registration

**`Extensions/ApplicationExtensions.cs`**

```csharp
public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        
        // Register MediatR with assembly scanning for handlers
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));
        
        // Register use case facades for backward compatibility
        services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
        services.AddScoped<IQueryTransactionUseCase, GetTransactionUseCase>();
        
        return services;
    }
}
```
### Directory Structure

```
Application/
├── Behaviors/
│   └── CreateTransactionCommand.cs
├── Extensions/
│   └── ApplicationExtensions.cs
├── UseCases/
|   |__GetPurchaseTransaction
|   |    |__GetPurchaseTransactionHandler
|   |    |__GetPurchaseTransactionRequest
|   |    |__GetPurchaseTransactionResponse
|   |__SavePurchaseTransaction
|        |__SaveTransactionCommand
|        |__SaveTransactionCommandHandler
└── GlobalUsings.cs
```

## MediatR Pipeline Behaviors

Pipeline behaviors centralize cross-cutting concerns (logging, validation, error handling) without cluttering individual handlers.

### Behavior Execution Order

```
Request → LoggingBehavior → ValidationBehavior → Handler → ErrorHandlingBehavior → Response
```

### LoggingBehavior

Logs request entry, execution time, and outcome. Uses Stopwatch for precise timing (<2ms overhead target).

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestType = typeof(TRequest).Name;
        
        Debug.WriteLine($"Processing request: {requestType}");
        
        try
        {
            var response = await next();
            stopwatch.Stop();
            Debug.WriteLine($"Request completed in {stopwatch.ElapsedMilliseconds}ms");
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Debug.WriteLine($"Request failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            throw;
        }
    }
}
```

### ValidationBehavior (Extensible Placeholder)

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Validating request: {typeof(TRequest).Name}");
        return await next();
    }
}
```

### ErrorHandlingBehavior

```csharp
public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in handler: {ex.Message}");
            // TODO: Phase 2C+ - Map exceptions to application layer
            throw;
        }
    }
}
```

### Pipeline Registration (ApplicationExtensions)

```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(ErrorHandlingBehavior<,>));
});
```

---

## Development Roadmap (Phases)

### Phase 2A (Current) ✓
- Core CQRS architecture with MediatR
- Commands and Queries abstractions
- Command and Query handlers
- MediatR Pipeline Behaviors (Logging, Validation, Error Handling)
- Documentation and examples

### Phase 2B (Future)
- API controller refactoring to use MediatR directly
- Remove dependency on use case facades
- FluentValidation integration for request validation
- Exception mapping to application-specific responses

### Phase 3+ (Future)
- Saga patterns for complex workflows
- Separate read/write models if scaling requires
- Event Sourcing (if needed for audit trails)

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