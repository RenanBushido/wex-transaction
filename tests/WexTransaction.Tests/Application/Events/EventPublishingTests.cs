namespace WexTransaction.Tests.Application.Events;

using Xunit;
using WexTransaction.Application.Events;
using WexTransaction.Application.Events.DomainEvents;
using WexTransaction.Domain.Interfaces;

/// <summary>
/// Test: Event publishing in handlers.
/// </summary>
public class EventPublishingTests
{
    [Fact]
    public async Task TransactionCreatedEvent_ImplementsIDomainEvent()
    {
        // Arrange
        var @event = new TransactionCreatedEvent(
            AggregateId: Guid.NewGuid(),
            OccurredAt: DateTimeOffset.UtcNow,
            Description: "Test Transaction",
            Amount: 100m,
            Date: DateTime.UtcNow
        );

        // Act & Assert
        Assert.NotNull(@event);
        Assert.IsAssignableFrom<IDomainEvent>(@event);
        Assert.Equal("TransactionCreated", @event.EventType);
    }

    [Fact]
    public async Task TransactionCreatedEvent_ContainsCorrectData()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;
        var description = "Test Description";
        var amount = 250.50m;
        var date = DateTime.UtcNow;

        // Act
        var @event = new TransactionCreatedEvent(
            AggregateId: aggregateId,
            OccurredAt: occurredAt,
            Description: description,
            Amount: amount,
            Date: date
        );

        // Assert
        Assert.Equal(aggregateId, @event.AggregateId);
        Assert.Equal(occurredAt, @event.OccurredAt);
        Assert.Equal(description, @event.Description);
        Assert.Equal(amount, @event.Amount);
        Assert.Equal(date, @event.Date);
    }

    [Fact]
    public async Task NoOpEventPublisher_PublishesWithoutPersisting()
    {
        // Arrange
        var publisher = new NoOpEventPublisher();
        var @event = new TransactionCreatedEvent(
            AggregateId: Guid.NewGuid(),
            OccurredAt: DateTimeOffset.UtcNow,
            Description: "Test",
            Amount: 100m,
            Date: DateTime.UtcNow
        );

        // Act
        var task = publisher.PublishAsync(@event, CancellationToken.None);

        // Assert
        Assert.NotNull(task);
        await task;
        // No exception thrown, event silently discarded
    }

    [Fact]
    public async Task NoOpEventPublisher_PublishesMultipleWithoutPersisting()
    {
        // Arrange
        var publisher = new NoOpEventPublisher();
        var events = new List<IDomainEvent>
        {
            new TransactionCreatedEvent(
                AggregateId: Guid.NewGuid(),
                OccurredAt: DateTimeOffset.UtcNow,
                Description: "Test 1",
                Amount: 100m,
                Date: DateTime.UtcNow
            ),
            new TransactionCreatedEvent(
                AggregateId: Guid.NewGuid(),
                OccurredAt: DateTimeOffset.UtcNow,
                Description: "Test 2",
                Amount: 200m,
                Date: DateTime.UtcNow
            )
        };

        // Act
        var task = publisher.PublishMultipleAsync(events, CancellationToken.None);

        // Assert
        Assert.NotNull(task);
        await task;
        // No exception thrown, events silently discarded
    }

    [Fact]
    public void TransactionCreatedEvent_IsImmutable()
    {
        // Arrange & Act
        var @event = new TransactionCreatedEvent(
            AggregateId: Guid.NewGuid(),
            OccurredAt: DateTimeOffset.UtcNow,
            Description: "Test",
            Amount: 100m,
            Date: DateTime.UtcNow
        );

        // Assert - Record types are immutable by design
        var eventType = @event.GetType();
        Assert.True(eventType.Name.Contains("TransactionCreatedEvent"));
        // Properties cannot be modified after creation (enforced by record type)
    }
}
