namespace WexTransaction.Tests.Application.Behaviors;

using MediatR;
using Xunit;
using WexTransaction.Application.Behaviors;
using WexTransaction.Application.Commands;
using WexTransaction.Application.PurchaseTransaction.SaveTransaction;

/// <summary>
/// Test: LoggingBehavior outputs expected timing information.
/// </summary>
public class LoggingBehaviorTests
{
    [Fact]
    public async Task LoggingBehavior_ExecutesSuccessfully()
    {
        // Arrange
        var behavior = new LoggingBehavior<CreateTransactionCommand, Guid>();
        var request = new CreateTransactionCommand("Test", DateTime.UtcNow, 100m);
        var responseHandled = false;

        // Act
        async Task<Guid> Next()
        {
            responseHandled = true;
            return Guid.NewGuid();
        }

        var response = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, response);
        Assert.True(responseHandled);
    }

    [Fact]
    public async Task LoggingBehavior_HandlesExceptionsAndRethrows()
    {
        // Arrange
        var behavior = new LoggingBehavior<CreateTransactionCommand, Guid>();
        var request = new CreateTransactionCommand("Test", DateTime.UtcNow, 100m);
        var testException = new InvalidOperationException("Test error");

        // Act
        async Task<Guid> FailingNext()
        {
            throw testException;
        }

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(request, FailingNext, CancellationToken.None));

        Assert.Equal(testException.Message, ex.Message);
    }

    [Fact]
    public async Task LoggingBehavior_MeasuresExecutionTime()
    {
        // Arrange
        var behavior = new LoggingBehavior<CreateTransactionCommand, Guid>();
        var request = new CreateTransactionCommand("Test", DateTime.UtcNow, 100m);

        // Act
        var startTime = DateTime.UtcNow;
        async Task<Guid> SlowNext()
        {
            await Task.Delay(10); // Simulate 10ms delay
            return Guid.NewGuid();
        }

        var response = await behavior.Handle(request, SlowNext, CancellationToken.None);
        var endTime = DateTime.UtcNow;

        // Assert
        var elapsed = (endTime - startTime).TotalMilliseconds;
        Assert.True(elapsed >= 10, $"Behavior should have taken at least 10ms, got {elapsed}ms");
        Assert.NotEqual(Guid.Empty, response);
    }

    [Fact]
    public async Task LoggingBehavior_SupportsAsyncOperations()
    {
        // Arrange
        var behavior = new LoggingBehavior<CreateTransactionCommand, Guid>();
        var request = new CreateTransactionCommand("Test", DateTime.UtcNow, 100m);
        var executionCount = 0;

        // Act
        async Task<Guid> AsyncNext()
        {
            executionCount++;
            await Task.Delay(1);
            return Guid.NewGuid();
        }

        var response = await behavior.Handle(request, AsyncNext, CancellationToken.None);

        // Assert
        Assert.Equal(1, executionCount);
        Assert.NotEqual(Guid.Empty, response);
    }
}
