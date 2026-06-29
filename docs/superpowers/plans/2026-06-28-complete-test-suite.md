# Complete Test Suite Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task. Each task is independently testable and should be executed with review checkpoints.

**Goal:** Implement missing test suites for Application Handlers, Validators, API Endpoints, and Documentation to achieve 80%+ coverage before Change Apply.

**Architecture:** 
- **Task 1-2:** SaveTransactionCommandHandler tests (write, persist, validation)
- **Task 3-4:** GetPurchaseTransactionHandler tests (read, conversion, null handling)
- **Task 5-6:** SaveTransactionValidator tests (field validation, edge cases)
- **Task 7-8:** AutoMapper MappingProfile tests (entity-to-DTO conversion)
- **Task 9-10:** API Endpoints tests (POST, GET with HTTP status codes)
- **Task 11-12:** GlobalExceptionHandler tests (exception mapping)
- **Task 13:** CLAUDE.md updates (CrossCutting pattern documentation)
- **Task 14:** Final validation (coverage report, all tests passing)

**Tech Stack:** xUnit, Moq, FluentAssertions, In-Memory EF Core DbContext, MediatR

## Global Constraints

- Minimum coverage target: 80%+ for core business logic (Domain, Application)
- Test pattern: Arrange-Act-Assert (AAA)
- Naming convention: `{Class}_{Scenario}_{Expected}` (e.g., `SaveTransactionCommandHandler_ValidInput_SavesAndReturnsId`)
- Use xUnit `[Fact]` for deterministic tests, `[Theory]` for parameterized tests
- All async methods tested with `async Task` pattern
- Domain exceptions must be caught and validated
- Mocks must be verified where behavior is tested

---

## Task 1: SaveTransactionCommandHandler - Valid Input Test

**Files:**
- Create: `tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionCommandHandlerTests.cs`
- Modify: None (new test class)

**Interfaces:**
- Consumes: 
  - `SaveTransactionCommand(Description, Date, Amount) : IRequest<Guid>`
  - `ITransactionRepository.SavePurchaseTransaction(transaction) : Task`
  - `IUnitOfWork.Commit(cancellationToken) : Task`
  - `SaveTransactionCommandHandler(repository, unitOfWork) : IRequestHandler<SaveTransactionCommand, Guid>`
  
- Produces:
  - Test class structure for handler testing with mocks
  - Baseline test for valid command handling

- [ ] **Step 1: Create test file with basic structure**

Create `tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionCommandHandlerTests.cs`:

```csharp
namespace WexTransaction.Tests.Application.UseCases.SavePurchaseTransaction;

public class SaveTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly SaveTransactionCommandHandler _handler;

    private static readonly DateTime ValidDate = new DateTime(2026, 6, 23, 8, 30, 0, DateTimeKind.Utc);
    private const string ValidDescription = "Coffee purchase";
    private const decimal ValidAmount = 200.24m;

    public SaveTransactionCommandHandlerTests()
    {
        _mockRepository = new Mock<ITransactionRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new SaveTransactionCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_SavesTransactionAndReturnsId()
    {
        // Arrange
        var command = new SaveTransactionCommand(ValidDescription, ValidDate, ValidAmount);
        
        // Mock setup
        _mockRepository
            .Setup(r => r.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()))
            .Returns(Task.CompletedTask);
        
        _mockUnitOfWork
            .Setup(u => u.Commit(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _mockRepository.Verify(
            r => r.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()), 
            Times.Once);
        _mockUnitOfWork.Verify(
            u => u.Commit(It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
```

- [ ] **Step 2: Run test to verify it passes**

```bash
cd /home/renan79/projetos/wex-transaction
dotnet test tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionCommandHandlerTests.cs -v
```

Expected output: `PASSED [... ms]`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionCommandHandlerTests.cs
git commit -m "test: add SaveTransactionCommandHandler valid input test"
```

---

## Task 2: SaveTransactionCommandHandler - Error Cases

**Files:**
- Modify: `tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionCommandHandlerTests.cs`

**Interfaces:**
- Consumes: Same as Task 1
- Produces: Extended test coverage for invalid inputs, repository failures, commit failures

- [ ] **Step 1: Add test for invalid description (empty)**

In `SaveTransactionCommandHandlerTests` class, add:

```csharp
[Fact]
public async Task Handle_WithEmptyDescription_ThrowsInvalidDescriptionException()
{
    // Arrange
    var command = new SaveTransactionCommand(string.Empty, ValidDate, ValidAmount);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidDescriptionException>(
        () => _handler.Handle(command, CancellationToken.None));
    
    _mockRepository.Verify(r => r.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()), Times.Never);
    _mockUnitOfWork.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
}

[Fact]
public async Task Handle_WithZeroAmount_ThrowsInvalidAmountException()
{
    // Arrange
    var command = new SaveTransactionCommand(ValidDescription, ValidDate, 0m);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidAmountException>(
        () => _handler.Handle(command, CancellationToken.None));
    
    _mockRepository.Verify(r => r.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()), Times.Never);
    _mockUnitOfWork.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
}

[Fact]
public async Task Handle_WithInvalidDate_ThrowsInvalidTransactionDateException()
{
    // Arrange
    var command = new SaveTransactionCommand(ValidDescription, default(DateTime), ValidAmount);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidTransactionDateException>(
        () => _handler.Handle(command, CancellationToken.None));
    
    _mockRepository.Verify(r => r.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()), Times.Never);
    _mockUnitOfWork.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
}
```

- [ ] **Step 2: Run tests to verify all pass**

```bash
dotnet test tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionCommandHandlerTests.cs -v
```

Expected output: `4 PASSED`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionCommandHandlerTests.cs
git commit -m "test: add SaveTransactionCommandHandler error case tests"
```

---

## Task 3: GetPurchaseTransactionHandler - Valid Request Test

**Files:**
- Create: `tests/WexTransaction.Tests/Application/UseCases/GetPurchaseTransaction/GetPurchaseTransactionHandlerTests.cs`
- Modify: None (new test class)

**Interfaces:**
- Consumes:
  - `GetPurchaseTransactionRequest(TransactionId, Country, Currency) : IRequest<GetPurchaseTransactionResponse>`
  - `ITransactionDapperRepository.GetByIdAsync(id) : Task<PurchaseTransaction>`
  - `IExchangeRateProvider.GetExchangeRatesAsync(country, currency) : Task<List<ExchangeRate>>`
  - `ExchangeRateSelector.Convert(transaction, rates) : ConvertedTransactionResult`
  - `GetPurchaseTransactionHandler(repository, exchangeRate) : IRequestHandler<...>`

- Produces:
  - Test class for handler with mocked dependencies
  - Test coverage for transaction retrieval and conversion

- [ ] **Step 1: Create test file with mock setup**

Create `tests/WexTransaction.Tests/Application/UseCases/GetPurchaseTransaction/GetPurchaseTransactionHandlerTests.cs`:

```csharp
namespace WexTransaction.Tests.Application.UseCases.GetPurchaseTransaction;

public class GetPurchaseTransactionHandlerTests
{
    private readonly Mock<ITransactionDapperRepository> _mockRepository;
    private readonly Mock<IExchangeRateProvider> _mockExchangeRateProvider;
    private readonly GetPurchaseTransactionHandler _handler;

    private static readonly DateTime TransactionDate = new DateTime(2026, 6, 23, 8, 30, 0, DateTimeKind.Utc);
    private static readonly Guid TransactionId = Guid.NewGuid();
    private const string Country = "Brazil";
    private const string Currency = "BRL";

    public GetPurchaseTransactionHandlerTests()
    {
        _mockRepository = new Mock<ITransactionDapperRepository>();
        _mockExchangeRateProvider = new Mock<IExchangeRateProvider>();
        _handler = new GetPurchaseTransactionHandler(_mockRepository.Object, _mockExchangeRateProvider.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequestAndExistingTransaction_ReturnsConvertedResponse()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Coffee", TransactionDate, 100m);
        var exchangeRates = new List<ExchangeRate>
        {
            new ExchangeRate(Currency, 5.25m, TransactionDate)
        };

        var request = new GetPurchaseTransactionRequest(transaction.Id, Country, Currency);

        _mockRepository
            .Setup(r => r.GetByIdAsync(transaction.Id))
            .ReturnsAsync(transaction);

        _mockExchangeRateProvider
            .Setup(e => e.GetExchangeRatesAsync(Country, Currency))
            .ReturnsAsync(exchangeRates);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.TransactionId);
        Assert.Equal((string)transaction.Description, result.Description);
        Assert.Equal((decimal)transaction.Amount, result.Amount);
        Assert.True(result.ConvertedValue > 0);

        _mockRepository.Verify(r => r.GetByIdAsync(transaction.Id), Times.Once);
        _mockExchangeRateProvider.Verify(e => e.GetExchangeRatesAsync(Country, Currency), Times.Once);
    }
}
```

- [ ] **Step 2: Run test to verify it passes**

```bash
dotnet test tests/WexTransaction.Tests/Application/UseCases/GetPurchaseTransaction/GetPurchaseTransactionHandlerTests.cs -v
```

Expected output: `PASSED [... ms]`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Application/UseCases/GetPurchaseTransaction/GetPurchaseTransactionHandlerTests.cs
git commit -m "test: add GetPurchaseTransactionHandler valid request test"
```

---

## Task 4: GetPurchaseTransactionHandler - Edge Cases

**Files:**
- Modify: `tests/WexTransaction.Tests/Application/UseCases/GetPurchaseTransaction/GetPurchaseTransactionHandlerTests.cs`

**Interfaces:**
- Consumes: Same as Task 3
- Produces: Extended test coverage for not found, null handling, invalid currency

- [ ] **Step 1: Add tests for transaction not found and invalid currency**

In `GetPurchaseTransactionHandlerTests` class, add:

```csharp
[Fact]
public async Task Handle_WithNonExistentTransactionId_ReturnsNull()
{
    // Arrange
    var nonExistentId = Guid.NewGuid();
    var request = new GetPurchaseTransactionRequest(nonExistentId, Country, Currency);

    _mockRepository
        .Setup(r => r.GetByIdAsync(nonExistentId))
        .ReturnsAsync((PurchaseTransaction)null!);

    // Act
    var result = await _handler.Handle(request, CancellationToken.None);

    // Assert
    Assert.Null(result);
    _mockRepository.Verify(r => r.GetByIdAsync(nonExistentId), Times.Once);
    _mockExchangeRateProvider.Verify(e => e.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
}

[Fact]
public async Task Handle_WithValidTransactionAndExchangeRates_ConvertsAmountCorrectly()
{
    // Arrange
    var transaction = PurchaseTransaction.Create("Book", TransactionDate, 50m);
    var exchangeRate = 3.50m; // 1 USD = 3.50 BRL
    var exchangeRates = new List<ExchangeRate>
    {
        new ExchangeRate(Currency, exchangeRate, TransactionDate)
    };

    var request = new GetPurchaseTransactionRequest(transaction.Id, Country, Currency);

    _mockRepository
        .Setup(r => r.GetByIdAsync(transaction.Id))
        .ReturnsAsync(transaction);

    _mockExchangeRateProvider
        .Setup(e => e.GetExchangeRatesAsync(Country, Currency))
        .ReturnsAsync(exchangeRates);

    // Act
    var result = await _handler.Handle(request, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    // Expected: 50m * 3.50m = 175m
    Assert.Equal(175m, result.ConvertedValue);
    Assert.Equal(exchangeRate, result.TaxRate);
}
```

- [ ] **Step 2: Run tests to verify all pass**

```bash
dotnet test tests/WexTransaction.Tests/Application/UseCases/GetPurchaseTransaction/GetPurchaseTransactionHandlerTests.cs -v
```

Expected output: `3 PASSED`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Application/UseCases/GetPurchaseTransaction/GetPurchaseTransactionHandlerTests.cs
git commit -m "test: add GetPurchaseTransactionHandler edge case tests"
```

---

## Task 5: SaveTransactionValidator Tests - Valid Input

**Files:**
- Create: `tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionValidatorTests.cs`
- Modify: None (new test class)

**Interfaces:**
- Consumes:
  - `SaveTransactionValidator : AbstractValidator<SaveTransactionCommand>`
  - Validation rules: Description (1-50 chars), Date (not null), Amount (not null)

- Produces:
  - Test class for validator with xUnit/FluentValidation patterns
  - Test coverage for all validation rules

- [ ] **Step 1: Create validator test file**

Create `tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionValidatorTests.cs`:

```csharp
namespace WexTransaction.Tests.Application.UseCases.SavePurchaseTransaction;

public class SaveTransactionValidatorTests
{
    private readonly SaveTransactionValidator _validator;

    private static readonly DateTime ValidDate = new DateTime(2026, 6, 23, 8, 30, 0, DateTimeKind.Utc);
    private const string ValidDescription = "Coffee purchase";
    private const decimal ValidAmount = 200.24m;

    public SaveTransactionValidatorTests()
    {
        _validator = new SaveTransactionValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new SaveTransactionCommand(ValidDescription, ValidDate, ValidAmount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyDescription_ReturnsFalse()
    {
        // Arrange
        var command = new SaveTransactionCommand(string.Empty, ValidDate, ValidAmount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WithDescriptionOver50Chars_ReturnsFalse()
    {
        // Arrange
        var longDescription = new string('x', 51);
        var command = new SaveTransactionCommand(longDescription, ValidDate, ValidAmount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WithNullDate_ReturnsFalse()
    {
        // Arrange
        var command = new SaveTransactionCommand(ValidDescription, default(DateTime), ValidAmount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Date");
    }

    [Fact]
    public void Validate_WithNullAmount_ReturnsFalse()
    {
        // Arrange
        var command = new SaveTransactionCommand(ValidDescription, ValidDate, 0m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var command = new SaveTransactionCommand(string.Empty, default(DateTime), 0m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }
}
```

- [ ] **Step 2: Run tests to verify they pass**

```bash
dotnet test tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionValidatorTests.cs -v
```

Expected output: `6 PASSED`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionValidatorTests.cs
git commit -m "test: add SaveTransactionValidator tests"
```

---

## Task 6: SaveTransactionValidator Tests - Boundary Cases

**Files:**
- Modify: `tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionValidatorTests.cs`

**Interfaces:**
- Consumes: Same as Task 5
- Produces: Extended test coverage for boundary values (exactly 50 chars, exactly 1 char, etc.)

- [ ] **Step 1: Add boundary and edge case tests**

In `SaveTransactionValidatorTests` class, add:

```csharp
[Fact]
public void Validate_WithExactly50CharDescription_ReturnsSuccess()
{
    // Arrange
    var description = new string('a', 50);
    var command = new SaveTransactionCommand(description, ValidDate, ValidAmount);

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.True(result.IsValid);
}

[Fact]
public void Validate_WithExactly1CharDescription_ReturnsSuccess()
{
    // Arrange
    var command = new SaveTransactionCommand("a", ValidDate, ValidAmount);

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.True(result.IsValid);
}

[Fact]
[Theory]
[InlineData(0.01)]
[InlineData(1000.00)]
[InlineData(999999.99)]
public void Validate_WithValidAmounts_ReturnsSuccess(decimal amount)
{
    // Arrange
    var command = new SaveTransactionCommand(ValidDescription, ValidDate, amount);

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.True(result.IsValid);
}
```

Wait, I made a syntax error. Let me fix it:

```csharp
[Theory]
[InlineData(0.01)]
[InlineData(1000.00)]
[InlineData(999999.99)]
public void Validate_WithVariousValidAmounts_ReturnsSuccess(decimal amount)
{
    // Arrange
    var command = new SaveTransactionCommand(ValidDescription, ValidDate, amount);

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.True(result.IsValid);
}
```

- [ ] **Step 2: Run tests to verify all pass**

```bash
dotnet test tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionValidatorTests.cs -v
```

Expected output: `9+ PASSED`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Application/UseCases/SavePurchaseTransaction/SaveTransactionValidatorTests.cs
git commit -m "test: add SaveTransactionValidator boundary case tests"
```

---

## Task 7: AutoMapper MappingProfile Tests

**Files:**
- Create: `tests/WexTransaction.Tests/Application/Mappings/MappingProfileTests.cs`
- Modify: None (new test class)

**Interfaces:**
- Consumes:
  - `MappingProfile : Profile`
  - AutoMapper configuration for Entity-to-DTO mappings
  - `PurchaseTransaction` entity with value objects
  - `GetPurchaseTransactionResponse` DTO

- Produces:
  - Test class for AutoMapper profile validation
  - Test coverage for entity-to-DTO conversion

- [ ] **Step 1: Create AutoMapper test file**

Create `tests/WexTransaction.Tests/Application/Mappings/MappingProfileTests.cs`:

```csharp
namespace WexTransaction.Tests.Application.Mappings;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void AutoMapperConfiguration_IsValid()
    {
        // Arrange & Act & Assert
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_PurchaseTransactionToGetPurchaseTransactionResponse_MapsSuccessfully()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Coffee", DateTime.UtcNow, 100m);

        // Act
        var response = _mapper.Map<GetPurchaseTransactionResponse>(transaction);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(transaction.Id, response.TransactionId);
        Assert.Equal((string)transaction.Description, response.Description);
        Assert.Equal((decimal)transaction.Amount, response.Amount);
        Assert.Equal(transaction.TransactionDate, response.Date);
    }

    [Fact]
    public void Map_PurchaseTransactionValueObjects_ConvertCorrectly()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Book purchase", DateTime.UtcNow, 50.25m);

        // Act
        var response = _mapper.Map<GetPurchaseTransactionResponse>(transaction);

        // Assert
        // Value object conversions via implicit operators
        Assert.Equal("Book purchase", response.Description);
        Assert.Equal(50.25m, response.Amount);
    }
}
```

- [ ] **Step 2: Run tests to verify they pass**

```bash
dotnet test tests/WexTransaction.Tests/Application/Mappings/MappingProfileTests.cs -v
```

Expected output: `3 PASSED`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Application/Mappings/MappingProfileTests.cs
git commit -m "test: add AutoMapper MappingProfile tests"
```

---

## Task 8: API Endpoints Tests - POST Transaction

**Files:**
- Create: `tests/WexTransaction.Tests/Api/Endpoints/TransactionEndpointsTests.cs`
- Modify: None (new test class)

**Interfaces:**
- Consumes:
  - `Endpoints.MapTransactionEndpoints(app) : void`
  - `SaveTransaction(request, mediator, cancellationToken) : Task<IResult>`
  - `GetTransaction(id, country, currency, mediator, cancellationToken) : Task<IResult>`
  - MediatR mediator for command/query dispatch

- Produces:
  - Test class for API endpoints with WebApplicationFactory
  - Test coverage for POST and GET endpoints

- [ ] **Step 1: Create endpoints test file**

Create `tests/WexTransaction.Tests/Api/Endpoints/TransactionEndpointsTests.cs`:

```csharp
namespace WexTransaction.Tests.Api.Endpoints;

public class TransactionEndpointsTests
{
    private readonly Mock<IMediator> _mockMediator;

    private static readonly DateTime ValidDate = new DateTime(2026, 6, 23, 8, 30, 0, DateTimeKind.Utc);
    private const string ValidDescription = "Coffee";
    private const decimal ValidAmount = 100.50m;

    public TransactionEndpointsTests()
    {
        _mockMediator = new Mock<IMediator>();
    }

    [Fact]
    public async Task SaveTransaction_WithValidRequest_ReturnsCreatedWith201()
    {
        // Arrange
        var request = new SaveTransactionRequest(ValidDescription, ValidDate, ValidAmount);
        var expectedId = Guid.NewGuid();
        
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SaveTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await Endpoints.SaveTransaction(request, _mockMediator.Object, CancellationToken.None);

        // Assert - Note: This tests the endpoint logic directly
        // The actual HTTP result would be Results.Created(...)
        Assert.NotNull(result);
        _mockMediator.Verify(
            m => m.Send(It.IsAny<SaveTransactionCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveTransaction_WithValidInput_DispatchesCorrectCommand()
    {
        // Arrange
        var request = new SaveTransactionRequest(ValidDescription, ValidDate, ValidAmount);
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SaveTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await Endpoints.SaveTransaction(request, _mockMediator.Object, CancellationToken.None);

        // Assert
        _mockMediator.Verify(
            m => m.Send(
                It.Is<SaveTransactionCommand>(cmd => 
                    cmd.Description == ValidDescription &&
                    cmd.Date == ValidDate &&
                    cmd.Amount == ValidAmount),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

- [ ] **Step 2: Run tests to verify they pass**

```bash
dotnet test tests/WexTransaction.Tests/Api/Endpoints/TransactionEndpointsTests.cs -v
```

Expected output: `2 PASSED`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Api/Endpoints/TransactionEndpointsTests.cs
git commit -m "test: add API Endpoints tests for POST transaction"
```

---

## Task 9: API Endpoints Tests - GET Transaction

**Files:**
- Modify: `tests/WexTransaction.Tests/Api/Endpoints/TransactionEndpointsTests.cs`

**Interfaces:**
- Consumes: Same as Task 8
- Produces: Extended test coverage for GET endpoint with various response scenarios

- [ ] **Step 1: Add GET endpoint tests**

In `TransactionEndpointsTests` class, add:

```csharp
[Fact]
public async Task GetTransaction_WithValidInput_DisptachesCorrectQuery()
{
    // Arrange
    var transactionId = Guid.NewGuid();
    var country = "Brazil";
    var currency = "BRL";
    var response = new GetPurchaseTransactionResponse(
        transactionId, "Coffee", DateTime.UtcNow, 100m, 5.25m, 525m);

    _mockMediator
        .Setup(m => m.Send(It.IsAny<GetPurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(response);

    // Act
    var result = await Endpoints.GetTransaction(
        transactionId, country, currency, _mockMediator.Object, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    _mockMediator.Verify(
        m => m.Send(
            It.Is<GetPurchaseTransactionRequest>(q =>
                q.TransactionId == transactionId &&
                q.Country == country &&
                q.Currency == currency),
            It.IsAny<CancellationToken>()),
        Times.Once);
}

[Fact]
public async Task GetTransaction_WithNonExistentId_ReturnsNotFound()
{
    // Arrange
    var transactionId = Guid.NewGuid();
    
    _mockMediator
        .Setup(m => m.Send(It.IsAny<GetPurchaseTransactionRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((GetPurchaseTransactionResponse)null!);

    // Act
    var result = await Endpoints.GetTransaction(
        transactionId, "Brazil", "BRL", _mockMediator.Object, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    _mockMediator.Verify(
        m => m.Send(It.IsAny<GetPurchaseTransactionRequest>(), It.IsAny<CancellationToken>()),
        Times.Once);
}
```

- [ ] **Step 2: Run tests to verify all pass**

```bash
dotnet test tests/WexTransaction.Tests/Api/Endpoints/TransactionEndpointsTests.cs -v
```

Expected output: `4 PASSED`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Api/Endpoints/TransactionEndpointsTests.cs
git commit -m "test: add API Endpoints tests for GET transaction"
```

---

## Task 10: GlobalExceptionHandler Tests

**Files:**
- Modify: `tests/WexTransaction.Tests/Api/GlobalExceptionHandlerTests.cs` (already exists)

**Interfaces:**
- Consumes:
  - `GlobalExceptionHandler : IExceptionHandler`
  - Exception mapping (DomainException → 422, ValidationException → 400, others → 500)
  - ProblemDetails response format

- Produces:
  - Extended test coverage for all exception types

- [ ] **Step 1: Read existing test file**

```bash
cat tests/WexTransaction.Tests/Api/GlobalExceptionHandlerTests.cs
```

- [ ] **Step 2: Add comprehensive exception mapping tests**

Modify `tests/WexTransaction.Tests/Api/GlobalExceptionHandlerTests.cs` to add:

```csharp
[Fact]
public async Task TryHandleAsync_WithDomainException_MapsTo422UnprocessableEntity()
{
    // Arrange
    var httpContext = new DefaultHttpContext();
    var exception = new InvalidDescriptionException("Description too long");
    var handler = new GlobalExceptionHandler();

    // Act
    var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

    // Assert
    Assert.True(result);
    Assert.Equal(StatusCodes.Status422UnprocessableEntity, httpContext.Response.StatusCode);
}

[Fact]
public async Task TryHandleAsync_WithValidationException_MapsTo400BadRequest()
{
    // Arrange
    var httpContext = new DefaultHttpContext();
    var validationException = new FluentValidation.ValidationException("Validation failed");
    var handler = new GlobalExceptionHandler();

    // Act
    var result = await handler.TryHandleAsync(httpContext, validationException, CancellationToken.None);

    // Assert
    Assert.True(result);
    Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
}

[Fact]
public async Task TryHandleAsync_WithUnhandledException_MapsTo500InternalServerError()
{
    // Arrange
    var httpContext = new DefaultHttpContext();
    var exception = new Exception("Unexpected error");
    var handler = new GlobalExceptionHandler();

    // Act
    var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

    // Assert
    Assert.True(result);
    Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
}
```

- [ ] **Step 2: Run tests to verify they pass**

```bash
dotnet test tests/WexTransaction.Tests/Api/GlobalExceptionHandlerTests.cs -v
```

Expected output: `All tests PASSED`

- [ ] **Step 3: Commit**

```bash
git add tests/WexTransaction.Tests/Api/GlobalExceptionHandlerTests.cs
git commit -m "test: enhance GlobalExceptionHandler tests with comprehensive exception mapping"
```

---

## Task 11: Update CLAUDE.md with CrossCutting Pattern Documentation

**Files:**
- Modify: `/home/renan79/projetos/wex-transaction/CLAUDE.md`

**Interfaces:**
- Consumes: CLAUDE.md existing content
- Produces: New section documenting CrossCutting/AppDependencies pattern

- [ ] **Step 1: Add CrossCutting Layer section to CLAUDE.md**

In CLAUDE.md, add a new section after "CQRS Pattern" or before "Development Roadmap":

```markdown
## CrossCutting Layer - Service Registration Pattern

The CrossCutting layer (`WexTransaction.CrossCutting/AppDependencies/`) centralizes all dependency injection extensions using the Service Registration Pattern.

### Location

```
WexTransaction.CrossCutting/
├── AppDependencies/
│   ├── ApplicationExtensions.cs      (Application services, MediatR, AutoMapper)
│   └── PersistenceExtensions.cs      (EF Core, Repositories, Unit of Work)
```

### Extension Files

**ApplicationExtensions.cs** — Registers application layer services:
- MediatR with assembly scanning for handlers
- AutoMapper with MappingProfile
- FluentValidation validators
- Pipeline behaviors (ValidationBehavior, UnhandledExceptionBehaviour)

**PersistenceExtensions.cs** — Registers infrastructure persistence services:
- DbContext (WexTransactionDbContext)
- Repositories (ITransactionRepository, ITransactionDapperRepository)
- Unit of Work (IUnitOfWork)

**ExternalApiExtensions.cs** — Registers external API integrations:
- Refit client (ITreasuryExchangeRateClient)
- Resilience policies (Polly: Retry, CircuitBreaker, Timeout)
- Exchange rate provider

### Pattern

Each extension file follows this structure:

```csharp
namespace WexTransaction.CrossCutting.AppDependencies;

public static class {Responsibility}Extensions
{
    public static IServiceCollection Add{Responsibility}(
        this IServiceCollection services,
        IConfiguration configuration) // optional if no config needed
    {
        // 1. Register main services
        services.AddScoped<IMyService, MyService>();
        
        // 2. Register dependent services
        services.AddScoped<IDependency, Implementation>();
        
        // 3. Return for fluent chaining
        return services;
    }
}
```

### Usage in Program.cs

All extensions are called in dependency order:

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Register all extensions in dependency order
builder.Services
    .AddPersistence(builder.Configuration)      // Infrastructure first
    .AddExternalApis(builder.Configuration)     // External APIs second
    .AddApplicationServices();                   // Application layer last

var app = builder.Build();

// ... rest of configuration
```

### Adding a New Service Extension

To add a new extension (e.g., Logging, Caching, Authentication):

1. Create file: `WexTransaction.CrossCutting/AppDependencies/{Responsibility}Extensions.cs`
2. Implement pattern: `Add{Responsibility}(this IServiceCollection, IConfiguration)`
3. Register in Program.cs in appropriate order (dependencies first)
4. Add tests in: `tests/WexTransaction.Tests/Infrastructure/Extensions/`

### Dependency Order

1. **Persistence** — DbContext, repositories (no external dependencies)
2. **ExternalApis** — Refit clients, resilience policies (depends on IConfiguration)
3. **Application** — MediatR, AutoMapper, validators (depends on persistence)
4. **Middleware/Handlers** — Exception handlers, logging (depends on all above)

```

- [ ] **Step 2: Commit changes**

```bash
git add CLAUDE.md
git commit -m "docs: add CrossCutting Layer service registration pattern documentation"
```

---

## Task 12: Run Full Test Suite and Validate Coverage

**Files:**
- Modify: None (validation only)

**Interfaces:**
- Consumes: All test files created in Tasks 1-10
- Produces: Coverage report (target 80%+)

- [ ] **Step 1: Run all tests**

```bash
cd /home/renan79/projetos/wex-transaction
dotnet test --verbosity normal
```

Expected output: All tests PASSED, 0 failures

- [ ] **Step 2: Generate coverage report**

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:Exclude="[*]*.Program"
```

Expected output: Coverage report generated (check for 80%+ in Domain and Application)

- [ ] **Step 3: Build solution to check for warnings**

```bash
dotnet build --no-incremental
```

Expected output: Build succeeded, 0 errors, minimal warnings

- [ ] **Step 4: Commit summary**

```bash
git add -A
git commit -m "test: complete test suite implementation - all tests passing"
```

---

## Task 13: Verify README.md Test Documentation

**Files:**
- Verify: `/home/renan79/projetos/wex-transaction/README.md` (already updated in previous step)

**Interfaces:**
- Consumes: Updated README.md from earlier review task
- Produces: Confirmation that test documentation is complete

- [ ] **Step 1: Verify README.md contains test documentation**

Check that README.md has:
- ✅ Testing Structure section with test layer organization
- ✅ Test Framework & Tools section (xUnit, Moq, etc.)
- ✅ Test Categories section (Domain, Application, Infrastructure, API)
- ✅ Running Tests section with actual commands
- ✅ Test Coverage Goals (target 80%+)

```bash
grep -c "Testing Structure\|Test Categories\|Running Tests\|dotnet test" README.md
```

Expected output: At least 4 matches

- [ ] **Step 2: No additional changes needed**

The README.md was updated in the previous review task and already contains comprehensive test documentation.

---

## Task 14: Final Validation and Cleanup

**Files:**
- Modify: `openspec/changes/ajustar-testes-projeto/tasks.md`

**Interfaces:**
- Consumes: All completed tasks (1-13)
- Produces: Updated tasks.md with completion status

- [ ] **Step 1: Update tasks.md to reflect completion**

Update `openspec/changes/ajustar-testes-projeto/tasks.md` with checkmarks:

Section 3: Application Layer Tests - Handlers
- [x] 3.1 SaveTransactionCommandHandler tests (COMPLETED)
- [x] 3.2 GetPurchaseTransactionQueryHandler tests (COMPLETED)
- [x] 3.3 Cobertura de handlers (COMPLETED)
- [x] 3.4 Validar testes (COMPLETED)

Section 5: Application Layer Tests - Validators
- [x] 5.1 SaveTransactionCommandValidator tests (COMPLETED)
- [x] 5.2 Mensagens de erro (COMPLETED)
- [x] 5.4 Rodar testes (COMPLETED)

Section 6: Application Layer Tests - Mapping
- [x] 6.1 MappingProfile tests (COMPLETED)
- [x] 6.2 Validar mapping (COMPLETED)

Section 10: API Layer Tests
- [x] 10.1 Endpoint tests (COMPLETED)
- [x] 10.2 GlobalExceptionHandler tests (COMPLETED)
- [x] 10.3 Validar testes (COMPLETED)

Section 13: Documentação
- [x] 13.1 Documentar estrutura de testes em README (COMPLETED)
- [x] 13.2-13.5 Guias e exemplos (COMPLETED)

- [ ] **Step 2: Final verification**

```bash
# Run final test suite
dotnet test --verbosity minimal

# Build check
dotnet build --no-restore

# Summary check
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover 2>&1 | grep -i coverage
```

Expected: All tests pass, build succeeds, coverage report generated

- [ ] **Step 3: Final commit**

```bash
git add openspec/changes/ajustar-testes-projeto/tasks.md
git commit -m "test: mark all test implementation tasks as complete - change ready for Apply"
```

---

## Post-Implementation Verification Checklist

After completing all tasks, verify:

- [ ] All 249+ tests passing: `dotnet test`
- [ ] Coverage 80%+ for core layers: Check coverage report
- [ ] Build succeeds: `dotnet build`
- [ ] README.md has complete test documentation
- [ ] CLAUDE.md has CrossCutting pattern documented
- [ ] tasks.md shows completion status (Section 1-11, 13 complete)
- [ ] No breaking changes to existing functionality
- [ ] All commits follow conventional commit format

## Next Steps

After all tasks are complete:

1. Run full test suite one more time: `dotnet test`
2. Generate final coverage report
3. Review git log for commit quality
4. Ready for OpenSpec Apply step
5. Can proceed with PR creation if needed
