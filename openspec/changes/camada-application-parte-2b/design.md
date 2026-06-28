## Context

Phase 2A established CQRS infrastructure. Phase 2B extends handlers to emit domain events and adds pipeline behaviors for cross-cutting concerns. Event abstractions provide contract for future Event Sourcing (Phase 3+) without requiring persistence implementation.

### Architectural Note (Clean Architecture Alignment)

This design aligns with the Clean Architecture refactoring (commit 0238c5a):
- **Ports (interfaces)** reside in `Domain/Interfaces/`
- **Implementations** reside in `Application/` or `Infrastructure/` layers
- **Dependency Inversion**: Higher layers depend on Domain abstractions

Phase 2B Pattern:
- `IDomainEvent` (port) → `Domain/Interfaces/IDomainEvent.cs`
- `IEventPublisher` (port) → `Domain/Interfaces/IEventPublisher.cs`
- `TransactionCreatedEvent` (impl) → `Application/Events/DomainEvents/TransactionCreatedEvent.cs`
- `NoOpEventPublisher` (impl) → `Application/Events/NoOpEventPublisher.cs`
- Behaviors (impl) → `Application/Behaviors/`

## Goals / Non-Goals

**Goals (Phase 2B):**
- Define IDomainEvent and IEventPublisher abstractions for domain event publishing
- Implement no-op event publisher (events not persisted yet)
- Create concrete domain events (TransactionCreatedEvent)
- Integrate event publishing into Phase 2A handlers
- Add MediatR pipeline behaviors for logging, validation, error handling
- Prepare foundation for Event Sourcing persistence (Phase 3)

**Non-Goals (Phase 2B):**
- Implement Event Sourcing persistence (deferred to Phase 3)
- Implement saga pattern orchestration (deferred to Phase 2C+)
- Split read/write databases (deferred)
- Add advanced distributed tracing (deferred)

## Decisions

### 1. Domain Event Abstraction
**Decision**: IDomainEvent interface with AggregateId, OccurredAt (DateTimeOffset UTC), EventType properties

**Rationale**:
- Standard event sourcing contract
- DateTimeOffset for proper UTC handling
- EventType enables event routing and filtering
- Immutable by using record types

### 2. Event Publisher Abstraction
**Decision**: IEventPublisher with PublishAsync and PublishMultipleAsync methods

**Rationale**:
- No-op implementation now, database-backed in Phase 3
- Supports batch publishing for future consistency guarantees
- Type-safe: requires IDomainEvent instances

### 3. No-Op Publisher Initially
**Decision**: NoOpEventPublisher discards events (no persistence yet)

**Rationale**:
- Establishes contract and pattern
- Allows Phase 2B to be complete even if Phase 3 not implemented yet
- Easy to swap for real implementation later

### 4. Pipeline Behaviors for Cross-Cutting Concerns
**Decision**: Implement LoggingBehavior as concrete example, ValidationBehavior and ErrorHandlingBehavior as extensible placeholders

**Rationale**:
- Centralizes logging without cluttering handlers
- Supports FluentValidation integration for future command validation
- Error handling behavior can map exceptions to HTTP responses
- MediatR built-in support for behaviors

### 5. Query Handlers: No Event Publishing in Phase 2B
**Decision**: Inject IEventPublisher in GetTransactionIdQueryHandler but DO NOT publish events

**Rationale**:
- Domain events model state changes (write operations)
- Queries represent read operations without state change
- Event publishing belongs in command handlers only
- Future: Phase 2C+ may add audit trail events if needed
- PreparedHook: IEventPublisher injection allows future query events without code changes

## Risks / Trade-offs

**Risk: Event Publishing No-Op Forgotten**
→ Mitigation: 
  - Code review checklist (task 10.2-10.3) catches missing event publishing
  - Unit tests verify PublishAsync called (task 9.5)
  - GitHub issue for Phase 3 prevents losing track

**Risk: Pipeline Behaviors Overhead**
→ Mitigation: 
  - Target < 5ms total overhead specified
  - LoggingBehavior uses efficient Stopwatch timing
  - Behaviors can be disabled in tests if needed
  - Performance measured in task 9.7

**Risk: Query Event Publishing Confusion**
→ Mitigation:
  - Design decision documented in Decision #5
  - Task 4.3 explicitly states NO publishing in queries
  - Task 8.4 verifies query handlers don't publish
  - CLAUDE.md documentation clarifies pattern

## Migration Path

1. **Phase 2A**: ✓ Core CQRS with MediatR
2. **Phase 2B** (this change): Event abstractions + pipeline behaviors + handler event publishing
3. **Phase 2C** (future): API controller refactoring to use MediatR directly
4. **Phase 3** (future): Event Sourcing persistence implementation
5. **Phase 4+** (future): Saga patterns, separate read/write models, etc.

## Implementation Scope (Phase 2B)

**New Directories (Clean Architecture Alignment):**
- `Domain/Interfaces/` - Ports: IDomainEvent, IEventPublisher (new interfaces)
- `Application/Events/` - Event implementations (NoOpEventPublisher, TransactionCreatedEvent)
- `Application/Events/DomainEvents/` - Concrete domain events
- `Application/Behaviors/` - Pipeline behavior implementations

**New Files:**

Domain Layer (Ports):
- `Domain/Interfaces/IDomainEvent.cs` - Port: domain event contract
- `Domain/Interfaces/IEventPublisher.cs` - Port: event publisher contract

Application Layer (Implementations):
- `Application/Events/NoOpEventPublisher.cs` - No-op publisher implementation
- `Application/Events/DomainEvents/TransactionCreatedEvent.cs` - Concrete event record
- `Application/Behaviors/LoggingBehavior.cs` - Request/response logging
- `Application/Behaviors/ValidationBehavior.cs` - Validation placeholder (extensible)
- `Application/Behaviors/ErrorHandlingBehavior.cs` - Error handling placeholder (extensible)

**Modified Files:**
- `Domain/GlobalUsings.cs` - Add System namespaces and async support
- `Application/GlobalUsings.cs` - Add Events and Behaviors namespaces
- `Application/Extensions/ApplicationExtensions.cs` - Register IEventPublisher and behaviors with MediatR
- `Application/PurchaseTransaction/SaveTransaction/SaveTransactionCommandHandler.cs` - Inject IEventPublisher, publish TransactionCreatedEvent
- `Application/PurchaseTransaction/GetByTransactionId/GetTransactionIdQueryHandler.cs` - Inject IEventPublisher (NO publishing in Phase 2B)
- `CLAUDE.md` - Add Event Sourcing and Pipeline Behaviors documentation

**Architecture Pattern:**
Follows Clean Architecture + Dependency Inversion:
- Ports (interfaces) in Domain/Interfaces/
- Implementations in Application/ or Infrastructure/
- Handlers delegate to services/behaviors via DI
