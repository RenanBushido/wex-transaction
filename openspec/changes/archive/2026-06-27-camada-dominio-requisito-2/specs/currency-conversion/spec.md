## ADDED Requirements

### Requirement: ExchangeRate value object holds a valid rate record
The `ExchangeRate` value object SHALL hold an immutable record from the Treasury Reporting Rates of Exchange dataset, consisting of `Country` (non-empty string), `Currency` (non-empty string), `Rate` (positive decimal), and `EffectiveDate` (UTC `DateTimeOffset`). Any attempt to construct an `ExchangeRate` with a null/empty country or currency SHALL raise an `ArgumentException`. Any attempt to construct with a non-positive rate or a non-UTC effective date SHALL raise an `ArgumentOutOfRangeException`.

#### Scenario: Valid rate is constructed
- **WHEN** `ExchangeRate` is created with a positive rate, non-empty country, non-empty currency, and a UTC date
- **THEN** the value object SHALL be created with all fields accessible

#### Scenario: Non-positive rate is rejected
- **WHEN** `ExchangeRate` is created with a rate of zero or a negative value
- **THEN** an `ArgumentOutOfRangeException` SHALL be thrown

#### Scenario: Non-UTC effective date is rejected
- **WHEN** `ExchangeRate` is created with an `EffectiveDate` whose offset is not `TimeSpan.Zero`
- **THEN** an `ArgumentOutOfRangeException` SHALL be thrown

#### Scenario: Null or empty Country is rejected
- **WHEN** `ExchangeRate` is created with a null, empty, or whitespace-only `Country`
- **THEN** an `ArgumentException` SHALL be thrown

#### Scenario: Null or empty Currency is rejected
- **WHEN** `ExchangeRate` is created with a null, empty, or whitespace-only `Currency`
- **THEN** an `ArgumentException` SHALL be thrown

#### Scenario: Two ExchangeRate instances with identical fields are equal
- **WHEN** two `ExchangeRate` instances are created with the same country, currency, rate, and effective date
- **THEN** they SHALL be considered equal (record value semantics)

### Requirement: Rate selector finds the most recent applicable rate within 6 months
The `ExchangeRateSelector` domain service SHALL select the most recent exchange rate whose `EffectiveDate` is less than or equal to the purchase transaction date AND is no older than 6 calendar months before the purchase transaction date. If multiple rates satisfy the window condition, the one with the latest `EffectiveDate` SHALL be chosen.

#### Scenario: Single qualifying rate is selected
- **WHEN** one rate has an `EffectiveDate` on or before the purchase date and within 6 months
- **THEN** that rate SHALL be used for conversion

#### Scenario: Multiple qualifying rates — most recent wins
- **WHEN** two or more rates have `EffectiveDate` within the 6-month window and on or before the purchase date
- **THEN** the rate with the latest `EffectiveDate` SHALL be selected

#### Scenario: Rate exactly on the purchase date is accepted
- **WHEN** a rate has an `EffectiveDate` equal to the purchase transaction date
- **THEN** that rate SHALL be selected as qualifying

#### Scenario: Rate with EffectiveDate exactly 6 calendar months before purchase date is accepted
- **WHEN** a rate has an `EffectiveDate` equal to `purchaseDate.AddMonths(-6)`
- **THEN** that rate SHALL still qualify (lower bound is inclusive)

#### Scenario: Rate with EffectiveDate one day after the 6-month lower bound is also accepted
- **WHEN** a rate has an `EffectiveDate` one day after `purchaseDate.AddMonths(-6)`
- **THEN** that rate SHALL qualify

#### Scenario: Rate after the purchase date is excluded
- **WHEN** a rate has an `EffectiveDate` strictly after the purchase transaction date
- **THEN** that rate SHALL NOT be selected, even if it is the only available rate

#### Scenario: Rate older than 6 calendar months is excluded
- **WHEN** a rate has an `EffectiveDate` strictly before `purchaseDate.AddMonths(-6)`
- **THEN** that rate SHALL NOT be selected

### Requirement: Conversion produces a result rounded to two decimal places
The `ExchangeRateSelector` SHALL compute the converted amount as `originalAmountUsd × selectedRate`, rounded to 2 decimal places using midpoint-away-from-zero rounding (`MidpointRounding.AwayFromZero`). The result SHALL be returned as a `ConvertedTransactionResult` containing the transaction identifier, description, date, original USD amount, the exchange rate used, and the converted amount.

#### Scenario: Converted amount is rounded to 2 decimal places
- **WHEN** the product of original amount and exchange rate has more than 2 decimal places (e.g., 200.24 × 5.25 = 1051.26)
- **THEN** the `ConvertedAmount` in the result SHALL be rounded to 2 decimal places

#### Scenario: Midpoint rounds away from zero
- **WHEN** the computed converted amount has exactly 5 as the third decimal digit (e.g., resulting in x.xx5)
- **THEN** the value SHALL round up (away from zero), not use banker's rounding

#### Scenario: Result includes all required fields
- **WHEN** `ExchangeRateSelector.Convert` is called with a valid transaction and a qualifying rate
- **THEN** the returned `ConvertedTransactionResult` SHALL contain the transaction's `Id`, `Description`, `TransactionDate`, `OriginalAmountUsd`, the selected `ExchangeRateUsed`, and `ConvertedAmount`

### Requirement: Conversion is unavailable when no qualifying rate exists within 6 months
If no exchange rate satisfying the 6-month window is available for the given purchase date, the `ExchangeRateSelector` SHALL throw a `CurrencyConversionUnavailableException` (a `DomainException` subclass). This exception SHALL be caught by the `GlobalExceptionHandler` and returned as an HTTP 422 Unprocessable Entity.

#### Scenario: Empty rate list throws CurrencyConversionUnavailableException
- **WHEN** `ExchangeRateSelector.Convert` is called with an empty collection of rates
- **THEN** a `CurrencyConversionUnavailableException` SHALL be thrown

#### Scenario: Only out-of-window rates throw CurrencyConversionUnavailableException
- **WHEN** all available rates are either after the purchase date or older than 6 calendar months
- **THEN** a `CurrencyConversionUnavailableException` SHALL be thrown

#### Scenario: CurrencyConversionUnavailableException is a DomainException
- **WHEN** a `CurrencyConversionUnavailableException` is thrown
- **THEN** it SHALL be an instance of `DomainException` so that `GlobalExceptionHandler` processes it as HTTP 422
