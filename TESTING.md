# WEX Transaction - Testing Guide

## Test Structure

The test project (`tests/WexTransaction.Tests/`) mirrors the source project structure (`src/WexTransaction/`) for easy navigation and maintenance.

```
tests/WexTransaction.Tests/
├── Domain/              # Domain layer tests
│   ├── Entities/
│   ├── Exceptions/
│   ├── Interfaces/
│   ├── Services/
│   └── ValueObjects/
├── Application/         # Application layer tests
│   ├── Behaviors/
│   ├── Extensions/
│   └── UseCases/
├── Infrastructure/      # Infrastructure layer tests
│   ├── Database/
│   │   ├── Config/
│   │   ├── Data/
│   │   ├── Extensions/
│   │   └── Repositories/
│   └── Services/
│       └── RatesExchange/
├── Api/                 # API layer tests
│   └── Endpoints/
└── CrossCutting/        # Shared test utilities
    ├── Fixtures/
    └── Helpers/
```

## Test Patterns

### Arrange-Act-Assert (AAA)

All tests follow the AAA pattern for clarity:

```csharp
[Fact]
public async Task Feature_Scenario_ExpectedResult()
{
    // Arrange: Setup test data and dependencies
    var command = new SaveTransactionCommand("Coffee", DateTime.UtcNow, 10.50m);
    var handler = new SaveTransactionCommandHandler(_repository, _unitOfWork);
    
    // Act: Execute the behavior being tested
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert: Verify the expected outcome
    Assert.NotEqual(Guid.Empty, result);
}
```

### Test Naming Convention

Tests use descriptive names following the pattern:
```
ClassName_Scenario_ExpectedResult
```

Examples:
- `SaveTransactionCommandHandler_ValidInput_ReturnsNonEmptyGuid`
- `ValidationBehavior_InvalidRequest_ThrowsValidationException`
- `GetTransactionQueryHandler_ExistingId_ReturnsTransaction`

## Test Categories

### 1. Domain Layer Tests
Tests for domain entities, value objects, exceptions, and business logic.

**Coverage**: Domain exceptions, Value Objects (Money, TransactionDescription), Domain Entities (PurchaseTransaction), Domain Interfaces (IUnitOfWork, ITransactionRepository, IExchangeRateProvider)

### 2. Application Layer Tests

#### Handler Tests (CQRS)
- **Command Handlers** (Write): SaveTransactionCommandHandler
- **Query Handlers** (Read): GetPurchaseTransactionQueryHandler
- Use in-memory database for isolation

#### Behavior Tests (MediatR Pipeline)
- **ValidationBehavior**: Ensures invalid requests are rejected before reaching handlers
- **UnhandledExceptionBehaviour**: Catches and logs unexpected exceptions
- Test isolation with mocks of the pipeline

#### Validator Tests (FluentValidation)
- Test each validator independently
- Validate both valid and invalid scenarios
- Ensure error messages are descriptive

#### Mapping Tests (AutoMapper)
- Test Entity → DTO mappings
- Validate value object conversions
- Verify ignored properties

### 3. Infrastructure Layer Tests

#### Database Tests (EF Core)
- **DbContext Tests**: CRUD operations with in-memory database
- **Configuration Tests**: Entity mapping, column names, types, required properties
- **Repository Tests**: Generic BaseRepository<T>, specific repositories

#### Dapper Tests (Hybrid)
- **Dapper Queries**: Direct SQL queries with SQLite temporary database
- **Hybrid Operations**: EF Core write, Dapper read coexistence
- **Parameter Binding**: SQL injection prevention

#### Extension Tests (DI)
- **PersistenceExtensions**: DbContext, repository registration
- **ExternalApiExtensions**: HTTP client, Refit, Polly policies
- **ApplicationExtensions**: MediatR, AutoMapper, validators

### 4. API Layer Tests
- **Endpoint Tests** (Minimal APIs): POST/GET transactions with valid/invalid input
- **Exception Handler Tests** (GlobalExceptionHandler): Mapping exceptions to HTTP responses

## Running Tests

### Run all tests
```bash
dotnet test tests/WexTransaction.Tests/WexTransaction.Tests.csproj
```

### Run tests for specific layer
```bash
dotnet test tests/WexTransaction.Tests/WexTransaction.Tests.csproj --filter "FullyQualifiedName~Domain"
dotnet test tests/WexTransaction.Tests/WexTransaction.Tests.csproj --filter "FullyQualifiedName~Application"
dotnet test tests/WexTransaction.Tests/WexTransaction.Tests.csproj --filter "FullyQualifiedName~Infrastructure"
```

### Run with coverage
```bash
dotnet test tests/WexTransaction.Tests/WexTransaction.Tests.csproj /p:CollectCoverage=true
```

### Run with verbose output
```bash
dotnet test tests/WexTransaction.Tests/WexTransaction.Tests.csproj -v detailed
```

## Test Dependencies

### Required Packages
- **xunit**: Unit testing framework
- **xunit.analyzers**: Code analysis for xUnit tests
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing
- **Moq**: Mocking library for isolating dependencies
- **FluentValidation**: Validation library testing
- **Microsoft.AspNetCore.TestHost**: HTTP testing utilities

### Available Fixtures
Use `CreateInMemoryContext()` helper for DbContext tests:
```csharp
private static WexTransactionDbContext CreateInMemoryContext()
{
    var options = new DbContextOptionsBuilder<WexTransactionDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    
    return new WexTransactionDbContext(options);
}
```

## Coverage Targets

- **Domain Layer**: 100% (critical business logic)
- **Application Handlers**: 95%+ (core use cases)
- **Application Behaviors**: 90%+ (cross-cutting)
- **Infrastructure**: 85%+ (repository, config, extensions)
- **API**: 80%+ (endpoints, error handling)
- **Overall**: 80%+ minimum

## Best Practices

1. **Isolation**: Each test should be independent and not rely on execution order
2. **Clarity**: Test names should clearly describe what is being tested
3. **Focus**: Each test should test one thing; use [Theory] for parametrized tests
4. **Speed**: Avoid external I/O; use mocks and in-memory databases
5. **Maintenance**: Keep tests as readable as production code
6. **Assertion**: Use specific assertions; avoid vague Assert.True()

## Troubleshooting

### Test hangs or timeouts
- Check for deadlocks in async operations
- Ensure CancellationToken is properly handled
- Verify mocks don't block unexpectedly

### In-memory database state issues
- Use `Guid.NewGuid().ToString()` for unique database names per test
- Seed data consistently before each test
- Clear DbContext properly between tests

### Validation test failures
- Verify validator rules match domain requirements
- Check error messages are accurate
- Ensure validator is registered in DI

## Adding New Tests

1. Create test file in appropriate folder matching src/ structure
2. Follow naming convention: `FeatureTests.cs`
3. Use [Fact] for single-scenario tests, [Theory] for parametrized
4. Follow AAA pattern
5. Use descriptive assertion messages
6. Run and verify coverage for new code

---

**Last Updated**: 2026-06-28  
**Coverage Target**: 80%+  
**Schema**: spec-driven (OpenSpec)
