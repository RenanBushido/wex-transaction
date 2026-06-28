namespace WexTransaction.Tests.Application.Behaviors;

using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using WexTransaction.Application.Behaviors;
using MediatR;

public class UnhandledExceptionBehaviourTests
{
    private readonly Mock<ILogger<TestRequest>> _mockLogger;
    private readonly UnhandledExceptionBehaviour<TestRequest, TestResponse> _behaviour;

    public UnhandledExceptionBehaviourTests()
    {
        _mockLogger = new Mock<ILogger<TestRequest>>();
        _behaviour = new UnhandledExceptionBehaviour<TestRequest, TestResponse>(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithSuccessfulRequest_PassesThroughWithoutLogging()
    {
        // Arrange
        var request = new TestRequest("Success");
        var response = new TestResponse("Success Response");
        RequestHandlerDelegate<TestResponse> next = () => Task.FromResult(response);

        // Act
        var result = await _behaviour.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal(response, result);
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithException_LogsErrorAndRethrows()
    {
        // Arrange
        var request = new TestRequest("Error");
        var testException = new InvalidOperationException("Test error message");
        RequestHandlerDelegate<TestResponse> next = () => throw testException;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _behaviour.Handle(request, next, CancellationToken.None));

        Assert.Equal(testException.Message, exception.Message);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e == testException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithException_LogsRequestName()
    {
        // Arrange
        var request = new TestRequest("Error");
        var testException = new ArgumentException("Argument error");
        RequestHandlerDelegate<TestResponse> next = () => throw testException;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _behaviour.Handle(request, next, CancellationToken.None));

        // Verify that the log message contains the request type name
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestRequest")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentExceptionTypes_LogsCorrectly()
    {
        // Arrange
        var request = new TestRequest("Error");

        // Test with different exception types
        Exception[] exceptionTypes =
        [
            new InvalidOperationException("Invalid operation"),
            new ArgumentNullException("Value cannot be null"),
            new TimeoutException("Operation timed out"),
            new NotImplementedException("Not implemented yet")
        ];

        foreach (var exception in exceptionTypes)
        {
            _mockLogger.Reset();
            RequestHandlerDelegate<TestResponse> next = () => throw exception;

            // Act & Assert
            await Assert.ThrowsAsync(exception.GetType(),
                () => _behaviour.Handle(request, next, CancellationToken.None));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(e => e == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_PropagatesCancellation()
    {
        // Arrange
        var request = new TestRequest("Cancel");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        RequestHandlerDelegate<TestResponse> next = () =>
            throw new OperationCanceledException("Operation was cancelled", cts.Token);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _behaviour.Handle(request, next, cts.Token));

        // Verify that the cancellation is logged as error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Helper classes for testing
    public record TestRequest(string Value) : IRequest<TestResponse>;
    public record TestResponse(string Value);
}
