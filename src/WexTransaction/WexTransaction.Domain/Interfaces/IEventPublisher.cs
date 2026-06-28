namespace WexTransaction.Domain.Interfaces;

/// <summary>
/// Port: Event publisher abstraction for domain event publishing.
///
/// Handlers use this port to publish domain events after successful operations.
/// Phase 2B: No-op implementation (events not persisted).
/// Phase 3: Will implement EventStorePublisher for persistence.
///
/// Design Principles:
/// - Type-safe: Requires IDomainEvent instances
/// - Async/await: Supports non-blocking operations
/// - Batch support: PublishMultipleAsync for future consistency guarantees
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish a single domain event asynchronously.
    /// </summary>
    /// <param name="domainEvent">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Publish multiple domain events asynchronously.
    /// Useful for atomic multi-event scenarios.
    /// </summary>
    /// <param name="domainEvents">The events to publish</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    Task PublishMultipleAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
