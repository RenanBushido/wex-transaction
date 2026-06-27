## 1. Domain Exceptions

- [x] 1.1 Create `src/WexTransaction/WexTransaction.Domain/Exceptions/DomainException.cs` — base exception class for all domain rule violations
- [x] 1.2 Create `src/WexTransaction/WexTransaction.Domain/Exceptions/InvalidDescriptionException.cs` — thrown when description is null, empty, or exceeds 50 characters
- [x] 1.3 Create `src/WexTransaction/WexTransaction.Domain/Exceptions/InvalidAmountException.cs` — thrown when amount is zero or negative
- [x] 1.4 Create `src/WexTransaction/WexTransaction.Domain/Exceptions/InvalidTransactionDateException.cs` — thrown when the transaction date is `default` or has a non-zero UTC offset (`Offset != TimeSpan.Zero`)

## 2. Value Objects

- [x] 2.1 Create `src/WexTransaction/WexTransaction.Domain/ValueObjects/TransactionDescription.cs` — `readonly record struct` wrapping a string; validates non-empty and max 50 characters; throws `InvalidDescriptionException` on violation
- [x] 2.2 Create `src/WexTransaction/WexTransaction.Domain/ValueObjects/Money.cs` — `readonly record struct` wrapping a decimal; validates positive value; rounds to 2 decimal places using `MidpointRounding.AwayFromZero`; throws `InvalidAmountException` on violation

## 3. Entity

- [x] 3.1 Create `src/WexTransaction/WexTransaction.Domain/Entities/PurchaseTransaction.cs` — aggregate root with properties `Id` (Guid), `Description` (TransactionDescription), `TransactionDate` (DateTimeOffset), `Amount` (Money); private parameterless constructor for EF Core; static factory method `Create(string description, DateTimeOffset transactionDate, decimal amount)` that validates: description via `TransactionDescription`, amount via `Money`, date not `default` AND `Offset == TimeSpan.Zero` (throws `InvalidTransactionDateException` otherwise), then assigns `Guid.NewGuid()` as the Id

## 4. Unit Tests

- [x] 4.1 Create `tests/WexTransaction.Tests/Domain/ValueObjects/TransactionDescriptionTests.cs` — tests for valid description (1 char, 50 chars), empty description rejection, description over 50 chars rejection
- [x] 4.2 Create `tests/WexTransaction.Tests/Domain/ValueObjects/MoneyTests.cs` — tests for positive amount acceptance, zero rejection, negative rejection, midpoint rounding (e.g., 1.005 → 1.01), standard rounding (e.g., 200.244 → 200.24)
- [x] 4.3 Create `tests/WexTransaction.Tests/Domain/Entities/PurchaseTransactionTests.cs` — tests for successful creation (all fields populated, Id not empty), default date rejection, non-UTC date rejection (e.g., offset `-03:00`), all individual field violations, two transactions having different Ids

## 5. Global Exception Handler

- [x] 5.1 Create `src/WexTransaction/WexTransaction.Api/Exceptions/GlobalExceptionHandler.cs` — implements `IExceptionHandler`; inject `IProblemDetailsService` via constructor and use `TryWriteAsync(...)` to write the ProblemDetails response; maps `InvalidAmountException`, `InvalidDescriptionException`, `InvalidTransactionDateException` → HTTP 400; maps base `DomainException` → HTTP 422; returns `false` for all other exception types
- [x] 5.2 Update `src/WexTransaction/WexTransaction.Api/Program.cs` — add `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()`, `builder.Services.AddProblemDetails()`, and `app.UseExceptionHandler()` before endpoint mapping
- [x] 5.3 Create `tests/WexTransaction.Tests/Api/GlobalExceptionHandlerTests.cs` — unit tests using `DefaultHttpContext` + `MemoryStream` response body: `InvalidAmountException` → 400, `InvalidDescriptionException` → 400, `InvalidTransactionDateException` → 400, base `DomainException` → 422, generic `Exception` → `TryHandleAsync` returns `false`; note: the scenario "non-domain exception produces 500" requires `WebApplicationFactory` (integration test, out of scope here)

## 6. Build Verification

- [x] 6.1 Run `dotnet build src/WexTransaction/WexTransaction.slnx` and confirm 0 errors
- [x] 6.2 Run `dotnet test` and confirm all new tests pass
