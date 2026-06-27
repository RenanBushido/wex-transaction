## ADDED Requirements

### Requirement: Application handles domain exceptions globally via IExceptionHandler
The `WexTransaction.Api` project SHALL register a `GlobalExceptionHandler` that implements `IExceptionHandler` and maps domain exceptions to RFC 7807 `ProblemDetails` responses. The handler SHALL be registered using `AddExceptionHandler<GlobalExceptionHandler>()` and `AddProblemDetails()` in `Program.cs`, with `UseExceptionHandler()` in the middleware pipeline.

#### Scenario: GlobalExceptionHandler is registered in the DI container
- **WHEN** the application starts
- **THEN** `IExceptionHandler` SHALL be resolvable from the DI container as `GlobalExceptionHandler`

#### Scenario: UseExceptionHandler middleware is in the pipeline
- **WHEN** the ASP.NET Core middleware pipeline is configured
- **THEN** `UseExceptionHandler()` SHALL be called before any endpoint middleware

### Requirement: Domain validation exceptions map to HTTP 400 ProblemDetails
When an `InvalidAmountException`, `InvalidDescriptionException`, or `InvalidTransactionDateException` is thrown during request processing, the `GlobalExceptionHandler` SHALL return an HTTP 400 Bad Request response with a `ProblemDetails` body containing the exception message as `detail`.

#### Scenario: InvalidAmountException returns 400
- **WHEN** an `InvalidAmountException` propagates to the middleware
- **THEN** the response SHALL have status code 400 and a `ProblemDetails` body with `title` "Bad Request" and `detail` containing the exception message

#### Scenario: InvalidDescriptionException returns 400
- **WHEN** an `InvalidDescriptionException` propagates to the middleware
- **THEN** the response SHALL have status code 400 and a `ProblemDetails` body with `title` "Bad Request" and `detail` containing the exception message

#### Scenario: InvalidTransactionDateException returns 400
- **WHEN** an `InvalidTransactionDateException` propagates to the middleware
- **THEN** the response SHALL have status code 400 and a `ProblemDetails` body with `title` "Bad Request" and `detail` containing the exception message

### Requirement: Unhandled domain exceptions map to HTTP 422
When a `DomainException` that is not one of the specific subtypes above propagates to the middleware, the `GlobalExceptionHandler` SHALL return HTTP 422 Unprocessable Entity.

#### Scenario: Generic DomainException returns 422
- **WHEN** a base `DomainException` (not a known subtype) propagates to the middleware
- **THEN** the response SHALL have status code 422 and a `ProblemDetails` body

### Requirement: Non-domain exceptions are not handled by GlobalExceptionHandler
The `GlobalExceptionHandler.TryHandleAsync` SHALL return `false` for any exception that is not a `DomainException` or its subtype, allowing ASP.NET Core's default fallback to produce a 500 Internal Server Error response.

#### Scenario: TryHandleAsync returns false for non-domain exceptions
- **WHEN** `TryHandleAsync` is called with an exception that is not a `DomainException`
- **THEN** the method SHALL return `false` without writing to the response

#### Scenario: Non-domain exception produces 500 in the middleware pipeline
- **WHEN** a generic `Exception` (not a `DomainException`) propagates through the full middleware stack
- **THEN** the response SHALL have status code 500
- **NOTE** this scenario requires an integration test with `WebApplicationFactory`; the unit test covers only the `false` return value
