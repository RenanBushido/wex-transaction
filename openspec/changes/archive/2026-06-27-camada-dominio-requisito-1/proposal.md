## Why

The application skeleton exists but has no domain model. Implementing Requirement #1 requires a `PurchaseTransaction` entity with its business rules encoded in the Domain layer before any persistence or API work can begin.

## What Changes

- Create the `PurchaseTransaction` entity in `WexTransaction.Domain` with properties: `Id` (Guid), `Description` (TransactionDescription), `TransactionDate` (DateTimeOffset), `Amount` (Money)
- Create a `Money` value object to encapsulate the positive-amount and rounding-to-nearest-cent invariant
- Create a `TransactionDescription` value object to encapsulate the max-50-character invariant
- Create domain exception types (`DomainException`, `InvalidAmountException`, `InvalidDescriptionException`, `InvalidTransactionDateException`) for validation failures
- Create a factory method `PurchaseTransaction.Create(...)` that enforces all field constraints (including UTC-only `DateTimeOffset`) and assigns the unique identifier
- Create `GlobalExceptionHandler` in `WexTransaction.Api` implementing `IExceptionHandler` (.NET 10) to map domain exceptions to `ProblemDetails` HTTP responses
- No EF Core, migrations, or repository concerns in this change

## Capabilities

### New Capabilities

- `purchase-transaction`: Domain entity `PurchaseTransaction` with its value objects, factory method, and validation rules enforcing the business requirements for description length, UTC transaction date, purchase amount, and unique identifier
- `global-exception-handler`: `GlobalExceptionHandler` in `WexTransaction.Api` that maps domain exceptions (`DomainException` and subtypes) to RFC 7807 `ProblemDetails` responses using `IExceptionHandler` + `IProblemDetailsService`

### Modified Capabilities

## Impact

- `src/WexTransaction/WexTransaction.Domain/` — entity, value objects, all exception types
- `src/WexTransaction/WexTransaction.Api/` — `GlobalExceptionHandler` and `Program.cs` registration
- `tests/WexTransaction.Tests/` — unit tests for domain rules and exception handler behavior
