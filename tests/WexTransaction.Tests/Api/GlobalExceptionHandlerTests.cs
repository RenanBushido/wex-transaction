using Microsoft.Extensions.Logging;

namespace WexTransaction.Tests.Api;

public class GlobalExceptionHandlerTests
{
    private static GlobalExceptionHandler CreateHandler(IProblemDetailsService? problemDetailsService = null)
    {
        var svc = problemDetailsService ?? new AlwaysTrueProblemDetailsService();
        var logger = new Mock<ILogger<GlobalExceptionHandler>>().Object;
        return new GlobalExceptionHandler(svc, logger);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task TryHandleAsync_InvalidAmountException_Returns400AndTrue()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new InvalidAmountException("Amount must be positive.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidDescriptionException_Returns400AndTrue()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new InvalidDescriptionException("Description too long.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidTransactionDateException_Returns400AndTrue()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new InvalidTransactionDateException("Date must be UTC.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_BaseDomainException_Returns417AndTrue()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new DomainException("Generic domain error.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status417ExpectationFailed, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_CurrencyConversionUnavailableException_Returns417AndTrue()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new CurrencyConversionUnavailableException("No rate available within 6 months.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status417ExpectationFailed, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_Returns500AndTrue()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Some infrastructure error.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_WithDomainException_MapsTo417UnprocessableEntity()
    {
        var context = CreateHttpContext();
        var exception = new DomainException("Generic domain error");
        var handler = CreateHandler();

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(StatusCodes.Status417ExpectationFailed, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_WithValidationException_MapsTo400BadRequest()
    {
        var context = CreateHttpContext();
        var validationException = new FluentValidation.ValidationException("Validation failed");
        var handler = CreateHandler();

        var result = await handler.TryHandleAsync(context, validationException, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnhandledException_MapsTo500InternalServerError()
    {
        var context = CreateHttpContext();
        var exception = new Exception("Unexpected error");
        var handler = CreateHandler();

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_LogsErrorWithExceptionType()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(new AlwaysTrueProblemDetailsService(), loggerMock.Object);
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Test error");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("InvalidOperationException")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_AcceptsILoggerDependency()
    {
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var svc = new AlwaysTrueProblemDetailsService();

        var exception = Record.Exception(() => new GlobalExceptionHandler(svc, loggerMock.Object));

        Assert.Null(exception);
    }

    private sealed class AlwaysTrueProblemDetailsService : IProblemDetailsService
    {
        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            context.HttpContext.Response.ContentType = "application/problem+json";
            return ValueTask.FromResult(true);
        }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            context.HttpContext.Response.ContentType = "application/problem+json";
            return ValueTask.CompletedTask;
        }
    }
}
