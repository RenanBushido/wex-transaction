namespace WexTransaction.Domain.Interfaces;

/// <summary>
/// Port: Domain event abstraction for event sourcing.
///
/// All domain events represent significant state changes in the system.
/// Events are immutable and use record types for safety.
///
/// Properties:
/// - AggregateId: The aggregate (entity) that triggered this event
/// - OccurredAt: When the event occurred (UTC)
/// - EventType: The type/name of this event (e.g., "TransactionCreated")
///
/// Persistence: Phase 3 will implement EventStore persistence.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The aggregate ID that triggered this event.
    /// </summary>
    Guid AggregateId { get; }

    /// <summary>
    /// The exact time this event occurred in UTC.
    /// </summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// The event type identifier (e.g., "TransactionCreated").
    /// Enables event routing and filtering.
    /// </summary>
    string EventType { get; }
}
