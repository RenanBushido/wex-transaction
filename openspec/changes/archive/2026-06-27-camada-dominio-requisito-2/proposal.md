## Why

Requirement #2 requires that a stored purchase transaction be retrievable in any currency supported by the Treasury Reporting Rates of Exchange API. The Domain layer currently has no model for exchange rates, currency conversion logic, or the result of a converted transaction — this change adds that domain vocabulary before the Application and Infrastructure layers can be built.

## What Changes

- Create `GlobalUsings.cs` in `WexTransaction.Domain` with all project-wide `global using` directives; remove individual `using` statements from all existing domain classes (Entities, ValueObjects, Exceptions)
- Create `ExchangeRate` value object: `CurrencyCode` (string), `Rate` (positive decimal), `EffectiveDate` (UTC `DateTimeOffset`)
- Create `ConvertedTransactionResult` value object: the read model returned after a successful conversion — includes `Id`, `Description`, `TransactionDate`, `OriginalAmountUsd` (`Money`), `ExchangeRate` (`ExchangeRate`), `ConvertedAmount` (`Money`)
- Create `CurrencyConversionUnavailableException : DomainException` — thrown when no valid exchange rate is found within 6 months of the purchase date
- Create `ExchangeRateSelector` domain service: given a `DateTimeOffset purchaseDate` and `IEnumerable<ExchangeRate> rates`, selects the most recent rate with `EffectiveDate ≤ purchaseDate` and within the last 6 calendar months; computes and returns a `ConvertedTransactionResult`

## Capabilities

### New Capabilities

- `currency-conversion`: domain rules for selecting the applicable exchange rate (most recent rate ≤ purchase date, within 6 calendar months) and computing the converted amount (rounded to 2 decimal places, `MidpointRounding.AwayFromZero`); includes `ExchangeRate` value object, `ConvertedTransactionResult` value object, `ExchangeRateSelector` domain service, and `CurrencyConversionUnavailableException`

### Modified Capabilities

## Impact

- `src/WexTransaction/WexTransaction.Domain/` — new `GlobalUsings.cs`; new value objects, exception, domain service; existing class files lose individual `using` statements
- `tests/WexTransaction.Tests/` — new unit tests for `ExchangeRate`, `ExchangeRateSelector`, and `ConvertedTransactionResult`
- No changes to Application, Infrastructure, or Api layers in this change
