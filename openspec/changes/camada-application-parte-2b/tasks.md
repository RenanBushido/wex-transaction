## 1. Create Event Abstractions (Ports in Domain Layer)

**Note**: Following Clean Architecture principles (per commit 0238c5a), ports (interfaces) reside in Domain/Interfaces/, implementations in Application layer.

- [ ] 1.1 Create IDomainEvent interface in Domain/Interfaces/
       Location: `/src/WexTransaction/WexTransaction.Domain/Interfaces/IDomainEvent.cs`
       Properties: AggregateId (Guid), OccurredAt (DateTimeOffset in UTC), EventType (string)
       Requirement: Immutable via record type

- [ ] 1.2 Create IEventPublisher interface in Domain/Interfaces/
       Location: `/src/WexTransaction/WexTransaction.Domain/Interfaces/IEventPublisher.cs`
       Methods: 
         - PublishAsync(IDomainEvent, CancellationToken)
         - PublishMultipleAsync(IEnumerable<IDomainEvent>, CancellationToken)
       Documentation: Port for event publishing infrastructure (persistence deferred to Phase 3)

- [ ] 1.3 Update Domain/GlobalUsings.cs
       Add: `global using WexTransaction.Domain.Interfaces;` (if not already present)

## 2. Create No-Op Event Publisher Implementation (Application Layer)

- [ ] 2.1 Create Events directory in Application project
       Location: `/src/WexTransaction/WexTransaction.Application/Events/`

- [ ] 2.2 Create NoOpEventPublisher implementing IEventPublisher
       Location: `/src/WexTransaction/WexTransaction.Application/Events/NoOpEventPublisher.cs`
       Namespace: `WexTransaction.Application.Events`
       Implementation:
         - PublishAsync: complete successfully without persisting
         - PublishMultipleAsync: complete successfully without persisting
       Add TODO comment: "Phase 3: Replace with EventStorePublisher for persistence"

- [ ] 2.3 Update Application/GlobalUsings.cs
       Add: `global using WexTransaction.Application.Events;`

## 3. Create Concrete Domain Events (Application Layer)

- [ ] 3.1 Create DomainEvents subdirectory in Application/Events
       Location: `/src/WexTransaction/WexTransaction.Application/Events/DomainEvents/`

- [ ] 3.2 Create TransactionCreatedEvent record implementing IDomainEvent
       Location: `/src/WexTransaction/WexTransaction.Application/Events/DomainEvents/TransactionCreatedEvent.cs`
       Namespace: `WexTransaction.Application.Events.DomainEvents`
       Properties: 
         - AggregateId (Guid) - from IDomainEvent
         - OccurredAt (DateTimeOffset) - from IDomainEvent
         - EventType (string) - from IDomainEvent, returns "TransactionCreated"
         - Description (string) - domain context
         - Amount (decimal) - domain context
         - Date (DateTime) - domain context
       Requirement: Record type for immutability

- [ ] 3.3 Verify immutability: all properties are read-only/init-only

## 4. Update Command Handlers with Event Publishing

- [ ] 4.1 Update SaveTransactionCommandHandler to inject IEventPublisher
       Location: `/src/WexTransaction/WexTransaction.Application/PurchaseTransaction/SaveTransaction/SaveTransactionCommandHandler.cs`
       Add constructor parameter: `IEventPublisher eventPublisher`
       Add field: `private readonly IEventPublisher _eventPublisher = eventPublisher;`

- [ ] 4.2 Add event publishing call after successful transaction save:
       Position: After `await _unitOfWork.Commit(cancellationToken);`
       Code:
       ```csharp
       var transactionCreatedEvent = new TransactionCreatedEvent(
           AggregateId: transaction.Id,
           OccurredAt: DateTimeOffset.UtcNow,
           Description: (string)transaction.Description,
           Amount: (decimal)transaction.Amount,
           Date: transaction.TransactionDate
       );
       await _eventPublisher.PublishAsync(transactionCreatedEvent, cancellationToken);
       ```
       Note: Cast value objects to scalar types for event data

- [ ] 4.3 GetTransactionIdQueryHandler: NO event publishing in Phase 2B
       Reason: Queries represent read operations; domain events model state changes (write operations)
       Future: Phase 2C+ may add query events if audit trail required
       Action: Inject IEventPublisher but do NOT call PublishAsync (leave as TODO comment)

## 5. Create MediatR Pipeline Behaviors (Application Layer)

- [ ] 5.1 Create Behaviors directory in Application project
       Location: `/src/WexTransaction/WexTransaction.Application/Behaviors/`

- [ ] 5.2 Create LoggingBehavior implementing IPipelineBehavior<TRequest, TResponse>
       Location: `/src/WexTransaction/WexTransaction.Application/Behaviors/LoggingBehavior.cs`
       Namespace: `WexTransaction.Application.Behaviors`
       Generic: `LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>`
       Implementation:
         - Inject ILogger<LoggingBehavior<TRequest, TResponse>>
         - Use Stopwatch to measure execution time
         - Log on entry: request type, timestamp
         - Call next() to invoke handler
         - Log on success: response type, execution time (ms), success status
         - Catch exceptions: log details, re-throw
       Performance: Target < 2ms overhead

- [ ] 5.3 Create ValidationBehavior as extensible placeholder
       Location: `/src/WexTransaction/WexTransaction.Application/Behaviors/ValidationBehavior.cs`
       Namespace: `WexTransaction.Application.Behaviors`
       Generic: `ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>`
       Phase 2B Implementation:
         - Inject ILogger<ValidationBehavior<TRequest, TResponse>>
         - Log: "Validating request: {RequestType}"
         - Call next() without validation
         - Add TODO comment: "Phase 2C: Integrate FluentValidation validators"
       Future: FluentValidation integration deferred to Phase 2C

- [ ] 5.4 Create ErrorHandlingBehavior as extensible placeholder
       Location: `/src/WexTransaction/WexTransaction.Application/Behaviors/ErrorHandlingBehavior.cs`
       Namespace: `WexTransaction.Application.Behaviors`
       Generic: `ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>`
       Phase 2B Implementation:
         - Inject ILogger<ErrorHandlingBehavior<TRequest, TResponse>>
         - Wrap next() in try-catch
         - Log exception: type, message, stack trace
         - Re-throw exception (mapping deferred to API layer)
         - Add TODO comment: "Phase 2C: Map domain exceptions to application exceptions"
       Future: Exception mapping deferred to Phase 2C

## 6. Register Event Publisher and Behaviors

- [ ] 6.1 Update Application/GlobalUsings.cs
       Add:
         - `global using WexTransaction.Application.Events;`
         - `global using WexTransaction.Application.Behaviors;`
       Verify: `global using WexTransaction.Domain.Interfaces;` already present

- [ ] 6.2 Update ApplicationExtensions.AddApplicationServices()
       Location: `/src/WexTransaction/WexTransaction.Application/Extensions/ApplicationExtensions.cs`
       
       Register IEventPublisher (port from Domain layer):
         ```csharp
         services.AddScoped<IEventPublisher, NoOpEventPublisher>();
         ```
       
       Update MediatR registration to include behaviors:
         ```csharp
         services.AddMediatR(config =>
         {
             config.RegisterServicesFromAssembly(typeof(SaveTransactionCommand).Assembly);
             config.AddOpenBehavior(typeof(LoggingBehavior<,>));
             config.AddOpenBehavior(typeof(ValidationBehavior<,>));
             config.AddOpenBehavior(typeof(ErrorHandlingBehavior<,>));
         });
         ```
       
       Behavior Execution Order: Logging → Validation → Handler → ErrorHandling

- [ ] 6.3 Verify fluent interface
       AddApplicationServices() must return IServiceCollection for method chaining

## 7. Update CLAUDE.md Documentation

- [ ] 7.1 Add "Event Sourcing Readiness (Phase 2B)" section to CLAUDE.md
       Location: After CQRS Pattern section
       Content:
         - Explain IDomainEvent port (in Domain/Interfaces/)
         - Show IDomainEvent contract: AggregateId, OccurredAt (DateTimeOffset UTC), EventType
         - Show TransactionCreatedEvent implementation example (record type)
         - Document IEventPublisher port contract: PublishAsync, PublishMultipleAsync
         - Document NoOpEventPublisher implementation (discards events)
         - Note: Event persistence deferred to Phase 3

- [ ] 7.2 Add "MediatR Pipeline Behaviors (Phase 2B)" section
       Content:
         - Explain IPipelineBehavior<TRequest, TResponse> pattern
         - Show behavior pipeline ordering: LoggingBehavior → ValidationBehavior → Handler → ErrorHandlingBehavior
         - Show LoggingBehavior example with Stopwatch for timing
         - Document validation behavior as extensible placeholder (FluentValidation Phase 2C+)
         - Document error handling behavior as extensible placeholder (exception mapping Phase 2C+)
         - Performance note: < 5ms total overhead target

- [ ] 7.3 Add "Event Publishing Pattern (Phase 2B)" section
       Content:
         - Show SaveTransactionCommandHandler example with IEventPublisher injection
         - Show event creation: new TransactionCreatedEvent(...)
         - Explain event correlation via AggregateId and OccurredAt
         - Show command → handler → event publishing flow
         - Clarify: Queries do NOT publish events (write operations only)

- [ ] 7.4 Update "Development Roadmap (Phases)" section
       Modify Phase 2B entry:
         ```markdown
         ### Phase 2B (Current)
         - Event abstractions (IDomainEvent, IEventPublisher in Domain/Interfaces)
         - Concrete domain events (TransactionCreatedEvent in Application/Events)
         - No-op event publisher implementation
         - MediatR pipeline behaviors (Logging, Validation, Error Handling)
         - Event publishing integration in command handlers
         - Documentation and examples
         ```

## 8. Verify Handler Event Publishing

- [ ] 8.1 Verify SaveTransactionCommandHandler compiles with IEventPublisher injection
       Check: Constructor has `IEventPublisher eventPublisher` parameter
       Check: Field `_eventPublisher` is assigned

- [ ] 8.2 Verify event publishing call in SaveTransactionCommandHandler
       Location: After `await _unitOfWork.Commit(cancellationToken);`
       Check: Uses `TransactionCreatedEvent` with correct properties
       Check: Passes AggregateId (transaction.Id), OccurredAt (DateTimeOffset.UtcNow)
       Check: Populates Description, Amount, Date from transaction

- [ ] 8.3 Verify event publishing is awaited
       Check: `await _eventPublisher.PublishAsync(...)` (not fire-and-forget)

- [ ] 8.4 Verify GetTransactionIdQueryHandler does NOT publish events
       Check: Has IEventPublisher injected (for future use)
       Check: NO PublishAsync() call in Handle method
       Check: TODO comment present for Phase 2C+ query events

## 9. Testing and Validation (Phase 2B)

- [ ] 9.1 Build verification
       Run: `dotnet build`
       Check: Solution compiles without errors
       Check: No compilation warnings in Application layer

- [ ] 9.2 MediatR registration verification
       Check: AddMediatR() discovers all behaviors
       Check: Services.AddScoped<IEventPublisher, NoOpEventPublisher>() registered
       Check: LoggingBehavior, ValidationBehavior, ErrorHandlingBehavior registered

- [ ] 9.3 Pipeline behavior ordering test
       Inject ILogger and verify log sequence:
         1. LoggingBehavior: "Request received: {RequestType}"
         2. ValidationBehavior: "Validating request: {RequestType}"
         3. Handler execution
         4. LoggingBehavior: "Request completed in Xms"

- [ ] 9.4 LoggingBehavior detailed test
       Test with successful handler: verify logs execution time, success status
       Test with exception: verify logs exception details and re-throws

- [ ] 9.5 Event publishing integration test
       Test SaveTransactionCommandHandler:
         - Mock IEventPublisher
         - Send SaveTransactionCommand via MediatR
         - Verify PublishAsync called once with TransactionCreatedEvent
         - Verify event data matches command input

- [ ] 9.6 NoOpEventPublisher verification
       Test: PublishAsync completes without errors
       Test: PublishMultipleAsync completes without errors
       Verify: No exceptions thrown, events silently discarded

- [ ] 9.7 Performance measurement
       Use Stopwatch in LoggingBehavior or test harness
       Measure: End-to-end request time with all behaviors
       Target: < 5ms total overhead
       Accept: < 3ms per behavior (logging ~1-2ms, validation ~0.5ms, error ~0.5ms)

## 10. Code Review Checklist

- [ ] 10.1 Architecture alignment
       Check: IDomainEvent in Domain/Interfaces/ ✅ (port)
       Check: IEventPublisher in Domain/Interfaces/ ✅ (port)
       Check: NoOpEventPublisher in Application/Events/ ✅ (implementation)
       Check: TransactionCreatedEvent in Application/Events/DomainEvents/ ✅ (implementation)
       Check: Pipeline behaviors in Application/Behaviors/ ✅ (implementation)
       Verify: Follows Clean Architecture (ports in Domain, implementations in Application)

- [ ] 10.2 Event implementation quality
       Check: All domain events implement IDomainEvent
       Check: All events use record type for immutability
       Check: All events set EventType (e.g., "TransactionCreated")
       Check: TransactionCreatedEvent has correct properties

- [ ] 10.3 Event publishing in SaveTransactionCommandHandler
       Check: IEventPublisher injected in constructor
       Check: PublishAsync called AFTER _unitOfWork.Commit (transactional)
       Check: PublishAsync is awaited (not fire-and-forget)
       Check: Event created with correct data

- [ ] 10.4 Pipeline behaviors
       Check: LoggingBehavior uses Stopwatch for timing
       Check: ValidationBehavior has Phase 2C TODO comment
       Check: ErrorHandlingBehavior has Phase 2C TODO comment
       Check: All behaviors are generic: <TRequest, TResponse>
       Check: All behaviors implement IPipelineBehavior

- [ ] 10.5 Dependency injection
       Check: IEventPublisher → NoOpEventPublisher registered
       Check: Pipeline behaviors registered with AddOpenBehavior
       Check: GlobalUsings updated with Events and Behaviors namespaces
       Check: Domain/GlobalUsings has Domain.Interfaces reference

- [ ] 10.6 DateTimeOffset usage
       Verify: OccurredAt uses DateTimeOffset.UtcNow (not DateTime.UtcNow)
       Reason: Proper UTC handling for event sourcing

- [ ] 10.7 Documentation
       Check: CLAUDE.md updated with Phase 2B section
       Check: Event publishing pattern documented
       Check: Pipeline behavior pipeline order explained
       Check: Examples are correct and runnable

- [ ] 10.8 Phase 3 preparation
       Action: Create GitHub issue for Phase 3
       Title: "Phase 3: Implement EventStorePublisher for event persistence"
       Description: Replace NoOpEventPublisher with EventStorePublisher to persist events to database
       Link to this change as prerequisite
