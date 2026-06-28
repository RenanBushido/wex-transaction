namespace WexTransaction.Application.Events.DomainEvents;

/// <summary>
/// Domain event: Triggered when a new purchase transaction is created.
/// Immutable record representing the transaction creation event.
/// </summary>
public record TransactionCreatedEvent(
    Guid AggregateId,
    DateTimeOffset OccurredAt,
    string Description,
    decimal Amount,
    DateTime Date
) : IDomainEvent
{
    /// <summary>
    /// The event type identifier for routing and filtering.
    /// </summary>
    public string EventType => "TransactionCreated";
}
