namespace WexTransaction.Tests.Application.Behaviors;

using System.Diagnostics;
using MediatR;
using Xunit;
using WexTransaction.Application.Behaviors;
using WexTransaction.Application.Commands;
using WexTransaction.Application.PurchaseTransaction.SaveTransaction;

/// <summary>
/// Test: Behavior performance overhead measurement.
/// Target: < 5ms total overhead per request
/// </summary>
public class BehaviorPerformanceTests
{
    [Fact]
    public async Task LoggingBehavior_PerformanceUnder2ms()
    {
        // Arrange
        var behavior = new LoggingBehavior<CreateTransactionCommand, Guid>();
        var request = new CreateTransactionCommand("Test", DateTime.UtcNow, 100m);
        const int iterations = 100;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            async Task<Guid> FastNext()
            {
                return Guid.NewGuid();
            }

            await behavior.Handle(request, FastNext, CancellationToken.None);
        }

        stopwatch.Stop();
        var averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert - Target < 2ms per request
        Assert.True(averageMs < 2.0, $"LoggingBehavior average: {averageMs}ms (should be < 2ms)");
    }

    [Fact]
    public async Task ValidationBehavior_PerformanceUnder1ms()
    {
        // Arrange
        var behavior = new ValidationBehavior<CreateTransactionCommand, Guid>();
        var request = new CreateTransactionCommand("Test", DateTime.UtcNow, 100m);
        const int iterations = 100;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            async Task<Guid> FastNext()
            {
                return Guid.NewGuid();
            }

            await behavior.Handle(request, FastNext, CancellationToken.None);
        }

        stopwatch.Stop();
        var averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert - Target < 1ms per request
        Assert.True(averageMs < 1.0, $"ValidationBehavior average: {averageMs}ms (should be < 1ms)");
    }

    [Fact]
    public async Task ErrorHandlingBehavior_PerformanceUnder1ms()
    {
        // Arrange
        var behavior = new ErrorHandlingBehavior<CreateTransactionCommand, Guid>();
        var request = new CreateTransactionCommand("Test", DateTime.UtcNow, 100m);
        const int iterations = 100;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            async Task<Guid> FastNext()
            {
                return Guid.NewGuid();
            }

            await behavior.Handle(request, FastNext, CancellationToken.None);
        }

        stopwatch.Stop();
        var averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert - Target < 1ms per request
        Assert.True(averageMs < 1.0, $"ErrorHandlingBehavior average: {averageMs}ms (should be < 1ms)");
    }

    [Fact]
    public async Task AllBehaviors_TotalOverheadUnder5ms()
    {
        // Arrange - Simulate all behaviors in pipeline
        var loggingBehavior = new LoggingBehavior<CreateTransactionCommand, Guid>();
        var validationBehavior = new ValidationBehavior<CreateTransactionCommand, Guid>();
        var errorHandlingBehavior = new ErrorHandlingBehavior<CreateTransactionCommand, Guid>();

        var request = new CreateTransactionCommand("Test", DateTime.UtcNow, 100m);
        const int iterations = 50;
        var stopwatch = Stopwatch.StartNew();

        // Act - Pipeline: ErrorHandling → Validation → Logging → Handler
        for (int i = 0; i < iterations; i++)
        {
            async Task<Guid> Pipeline()
            {
                return await errorHandlingBehavior.Handle(
                    request,
                    async () => await validationBehavior.Handle(
                        request,
                        async () => await loggingBehavior.Handle(
                            request,
                            async () => Guid.NewGuid(),
                            CancellationToken.None),
                        CancellationToken.None),
                    CancellationToken.None);
            }

            await Pipeline();
        }

        stopwatch.Stop();
        var averageMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

        // Assert - Target < 5ms total overhead
        Assert.True(averageMs < 5.0, $"Pipeline average: {averageMs}ms (should be < 5ms)");
    }

    [Fact]
    public async Task SingleBehavior_DoesNotBlockThread()
    {
        // Arrange
        var behavior = new LoggingBehavior<CreateTransactionCommand, Guid>();
        var request = new CreateTransactionCommand("Test", DateTime.UtcNow, 100m);

        // Act
        var task1 = behavior.Handle(request, async () => Guid.NewGuid(), CancellationToken.None);
        var task2 = behavior.Handle(request, async () => Guid.NewGuid(), CancellationToken.None);
        var task3 = behavior.Handle(request, async () => Guid.NewGuid(), CancellationToken.None);

        // Assert - All tasks should complete without blocking
        await Task.WhenAll(task1, task2, task3);
        Assert.NotEqual(Guid.Empty, await task1);
        Assert.NotEqual(Guid.Empty, await task2);
        Assert.NotEqual(Guid.Empty, await task3);
    }
}
