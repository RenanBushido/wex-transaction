## Context

The Domain layer models `PurchaseTransaction` and its value objects (`Money`, `TransactionDescription`) but has no vocabulary for exchange rates or currency conversion. Requirement #2 introduces conversion rules that are pure business logic — rate selection window, ordering, rounding — and must live in the Domain before Application or Infrastructure layers can implement the full flow.

The existing domain uses `readonly record struct` for value objects, a factory method on the entity, and typed `DomainException` subclasses for invariant violations. This change follows the same patterns.

## Goals / Non-Goals

**Goals:**
- Add `ExchangeRate` value object to represent a single rate record from Treasury API data
- Add `ConvertedTransactionResult` as an immutable domain result type
- Add `ExchangeRateSelector` domain service encapsulating rate-selection and conversion logic
- Add `CurrencyConversionUnavailableException` for when no valid rate exists within 6 months
- Centralize all `using` directives in `GlobalUsings.cs` within `WexTransaction.Domain`

**Non-Goals:**
- Fetching rates from Treasury API (Infrastructure port/adapter)
- HTTP request/response mapping (Api/Application layer)
- Persisting exchange rates (not stored in the database)
- Defining the Application use case or the Infrastructure Refit client

## Decisions

### ExchangeRate as `readonly record struct`
Follows the same pattern as `Money` and `TransactionDescription`. Exchange rates are immutable data carriers with value semantics — two rates are equal when all fields match. Fields: `string Country`, `string Currency`, `decimal Rate` (must be > 0), `DateTimeOffset EffectiveDate` (must be UTC). No separate `CurrencyCode` wrapper — the Treasury API uses separate `country` and `currency` string fields, so the domain models them separately.

Validation throws `ArgumentException` for null/empty/whitespace `Country` or `Currency`, and `ArgumentOutOfRangeException` for non-positive `Rate` and for non-UTC `EffectiveDate`. These are infrastructure-level construction errors (bad API response parsing), not user-input domain violations, so they don't need a custom `DomainException` subclass.

### ConvertedTransactionResult as `sealed record`
The result combines data from two sources (transaction and rate). It is a pure read model with no identity or persistence. A `sealed record` (reference type) is preferred over `struct` here to avoid large-struct copying — it has multiple decimal fields plus strings. Fields: `Guid TransactionId`, `string Description`, `DateTimeOffset TransactionDate`, `decimal OriginalAmountUsd`, `decimal ExchangeRateUsed`, `decimal ConvertedAmount`.

Using `decimal` (not `Money`) for `OriginalAmountUsd`, `ExchangeRate`, and `ConvertedAmount` keeps the result type a thin DTO that the Application layer can map directly. The invariant (`ConvertedAmount > 0`) is guaranteed by construction since both `OriginalAmountUsd` and `ExchangeRate` are already validated as positive.

### ExchangeRateSelector as a static class
Rate selection and conversion is a pure function with no external dependencies and no mutable state. A static class avoids DI complexity while keeping the logic discoverable and testable via direct method calls. If a future requirement needs this to be injectable (e.g., pluggable rounding strategy), an interface can be extracted without changing the domain logic.

The single public method: `static ConvertedTransactionResult Convert(PurchaseTransaction transaction, IEnumerable<ExchangeRate> rates)`.

### 6-month window using `AddMonths(-6)`
"Within the last 6 months equal to or before the purchase date" maps to:
```
lowerBound = purchaseDate.AddMonths(-6)
rate.EffectiveDate >= lowerBound && rate.EffectiveDate <= purchaseDate
```
Calendar months (`AddMonths`) match financial reporting conventions and the expected behavior of the Treasury Reporting Rates of Exchange dataset (which uses quarterly/monthly effective dates).

### Rounding: `Math.Round(..., 2, MidpointRounding.AwayFromZero)`
Consistent with `Money` rounding. The converted amount is: `Math.Round(originalAmount * rate, 2, MidpointRounding.AwayFromZero)`.

### GlobalUsings.cs in WexTransaction.Domain
A single `GlobalUsings.cs` at the root of the project declares all `global using` directives. All individual `using` statements are removed from existing `.cs` files. The C# compiler merges global usings at build time — no runtime effect, purely a code organization improvement.

## Risks / Trade-offs

- **`AddMonths(-6)` edge case at month boundaries** → Accepted. `DateTime.AddMonths` handles month-length differences correctly (e.g., March 31 → September 30). This is the standard .NET approach.
- **Static `ExchangeRateSelector` is hard to mock in isolation** → Low risk at domain layer; Application layer tests will mock the port that provides rates, not the selector.
- **`ConvertedTransactionResult` exposes `decimal` fields** → The Application layer must not confuse `ExchangeRate` (the field) with the `ExchangeRate` (the type). Renaming the field to `ExchangeRateUsed` removes the ambiguity.

## Open Questions

None — all design decisions are resolved and can be implemented immediately.
