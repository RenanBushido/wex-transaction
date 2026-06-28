namespace WexTransaction.Application.Events;

/// <summary>
/// No-op implementation of IEventPublisher for Phase 2B.
///
/// Events are silently discarded (not persisted).
/// This allows Phase 2B to be feature-complete while deferring persistence to Phase 3.
///
/// Phase 3: Replace with EventStorePublisher to persist events to database.
/// </summary>
public class NoOpEventPublisher : IEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // TODO: Phase 3 - Replace with EventStorePublisher for persistence
        return Task.CompletedTask;
    }

    public Task PublishMultipleAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        // TODO: Phase 3 - Replace with EventStorePublisher for persistence
        return Task.CompletedTask;
    }
}
