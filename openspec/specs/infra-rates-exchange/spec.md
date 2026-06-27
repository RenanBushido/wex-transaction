# infra-rates-exchange

## Purpose

External API integration layer (Refit) for Treasury Reporting Rates of Exchange API with caching and error handling.

## Requirements

### Requirement: Refit client for Treasury Reporting Rates of Exchange API

The system SHALL provide a Refit HTTP client for integrating with the Treasury Reporting Rates of Exchange API.

#### Scenario: Client is configured with base URL
- **WHEN** the HTTP client is initialized
- **THEN** it is configured with the Treasury API base URL (https://fiscaldata.treasury.gov)

#### Scenario: Client has appropriate timeout
- **WHEN** a request is made to the Treasury API
- **THEN** the request times out after a configured duration (default 30 seconds)

#### Scenario: Client methods are async
- **WHEN** rate data is fetched
- **THEN** the client methods return Task<T> for async/await usage

### Requirement: Exchange rate provider implementation

The system SHALL provide an implementation of IExchangeRateProvider that fetches exchange rates from the Treasury API.

#### Scenario: Provider fetches rates by country and currency
- **WHEN** IExchangeRateProvider.GetExchangeRatesAsync() is called with country and currency parameters
- **THEN** the method calls the Treasury API and returns a collection of ExchangeRate objects

#### Scenario: Provider handles API errors gracefully
- **WHEN** the Treasury API returns an error or is unavailable
- **THEN** the provider throws CurrencyConversionUnavailableException for the Application layer to handle

#### Scenario: Provider maps API response to domain ExchangeRate
- **WHEN** the Treasury API returns exchange rate data
- **THEN** the response is mapped to ExchangeRate value objects with Country, Currency, Rate, and EffectiveDate

#### Scenario: Provider caches rates to reduce API calls
- **WHEN** the same country/currency is requested multiple times within a short period
- **THEN** cached results are returned instead of calling the API again

### Requirement: Service registration via extension

The system SHALL register all external API services in the DI container through an extension method in the Extensions folder.

#### Scenario: ExternalApiExtensions registers Refit client
- **WHEN** ExternalApiExtensions.AddExternalApis() is called
- **THEN** the Refit client and IExchangeRateProvider are registered in IServiceCollection

#### Scenario: API base URL is read from configuration
- **WHEN** AddExternalApis() is invoked
- **THEN** it reads the Treasury API URL from IConfiguration

#### Scenario: HTTP client is registered with timeout
- **WHEN** the HTTP client is registered
- **THEN** it has a timeout value configured from IConfiguration or defaults to 30 seconds
