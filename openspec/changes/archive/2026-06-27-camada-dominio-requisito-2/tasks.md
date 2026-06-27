## 1. Global Usings

- [x] 1.1 Create `src/WexTransaction/WexTransaction.Domain/GlobalUsings.cs` with `global using` directives only for project-own namespaces (`WexTransaction.Domain.Entities`, `WexTransaction.Domain.Exceptions`, `WexTransaction.Domain.ValueObjects`, `WexTransaction.Domain.Services`) — do NOT add system namespaces already covered by `<ImplicitUsings>enable</ImplicitUsings>` in the csproj
- [x] 1.2 Remove all individual `using` statements from `Entities/PurchaseTransaction.cs`, `ValueObjects/Money.cs`, `ValueObjects/TransactionDescription.cs`, and all `Exceptions/*.cs` files

## 2. Domain Exception

- [x] 2.1 Create `src/WexTransaction/WexTransaction.Domain/Exceptions/CurrencyConversionUnavailableException.cs` as a sealed subclass of `DomainException`

## 3. Value Objects

- [x] 3.1 Create `src/WexTransaction/WexTransaction.Domain/ValueObjects/ExchangeRate.cs` as a `readonly record struct` with `string Country`, `string Currency`, `decimal Rate` (> 0, throws `ArgumentOutOfRangeException`), and `DateTimeOffset EffectiveDate` (UTC, throws `ArgumentOutOfRangeException`)
- [x] 3.2 Create `src/WexTransaction/WexTransaction.Domain/ValueObjects/ConvertedTransactionResult.cs` as a `sealed record` with `Guid TransactionId`, `string Description`, `DateTimeOffset TransactionDate`, `decimal OriginalAmountUsd`, `decimal ExchangeRateUsed`, and `decimal ConvertedAmount`

## 4. Domain Service

- [x] 4.1 Create `src/WexTransaction/WexTransaction.Domain/Services/ExchangeRateSelector.cs` as a static class with method `static ConvertedTransactionResult Convert(PurchaseTransaction transaction, IEnumerable<ExchangeRate> rates)` — filters rates where `EffectiveDate <= transaction.TransactionDate` and `EffectiveDate >= transaction.TransactionDate.AddMonths(-6)`, orders by `EffectiveDate` descending, takes first or throws `CurrencyConversionUnavailableException`, then returns a `ConvertedTransactionResult` with `ConvertedAmount = Math.Round(transaction.Amount.Value * selectedRate.Rate, 2, MidpointRounding.AwayFromZero)`

## 5. Unit Tests

- [x] 5.1 Create `tests/WexTransaction.Tests/Domain/ExchangeRateTests.cs` — covers valid construction, non-positive rate rejection, non-UTC date rejection, empty/null Country rejection, empty/null Currency rejection, and value equality
- [x] 5.2 Create `tests/WexTransaction.Tests/Domain/ExchangeRateSelectorTests.cs` — covers: single qualifying rate selected, most recent rate selected when multiple qualify, rate on purchase date accepted, rate exactly 6 months before accepted, rate one day inside window accepted, rate after purchase date excluded, rate older than 6 months excluded, empty list throws `CurrencyConversionUnavailableException`, only out-of-window rates throws `CurrencyConversionUnavailableException`, result fields populated correctly, midpoint rounding away from zero
- [x] 5.3 Create `tests/WexTransaction.Tests/Domain/ConvertedTransactionResultTests.cs` — covers record equality and that all constructor fields are accessible
- [x] 5.4 Add test `TryHandleAsync_CurrencyConversionUnavailableException_Returns422AndTrue` to `tests/WexTransaction.Tests/Api/GlobalExceptionHandlerTests.cs`

## 6. Build Verification

- [x] 6.1 Run `dotnet build src/WexTransaction/WexTransaction.slnx` — zero errors and zero warnings
- [x] 6.2 Run `dotnet test tests/WexTransaction.Tests/WexTransaction.Tests.csproj` — all tests pass
