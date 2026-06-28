namespace WexTransaction.Tests.Application.Behaviors;

/// <summary>
/// Test: ValidationBehavior extensible placeholder.
/// </summary>
public class ValidationBehaviorTests
{
    [Fact]
    public async Task ValidationBehavior_PassesThroughToNextHandler()
    {
        // Arrange
        var behavior = new ValidationBehavior<SaveTransactionCommand, Guid>();
        var request = new SaveTransactionCommand("Test", DateTime.UtcNow, 100m);
        var handlerExecuted = false;

        // Act
        async Task<Guid> Next()
        {
            handlerExecuted = true;
            return Guid.NewGuid();
        }

        var response = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.True(handlerExecuted);
        Assert.NotEqual(Guid.Empty, response);
    }

    [Fact]
    public async Task ValidationBehavior_DoesNotThrowOnValidRequest()
    {
        // Arrange
        var behavior = new ValidationBehavior<SaveTransactionCommand, Guid>();
        var validRequest = new SaveTransactionCommand("Valid", DateTime.UtcNow, 50m);

        // Act & Assert - Should not throw
        async Task<Guid> Next()
        {
            return Guid.NewGuid();
        }

        var response = await behavior.Handle(validRequest, Next, CancellationToken.None);
        Assert.NotEqual(Guid.Empty, response);
    }

    [Fact]
    public async Task ValidationBehavior_IsPlaceholderForPhase2C()
    {
        // Arrange
        var behavior = new ValidationBehavior<SaveTransactionCommand, Guid>();

        // Act & Assert - Verify it's a placeholder (passes through without validation)
        // Phase 2C will integrate FluentValidation
        var request = new SaveTransactionCommand("", DateTime.UtcNow, -100m); // Invalid data

        async Task<Guid> Next()
        {
            return Guid.NewGuid();
        }

        // Should NOT throw because Phase 2B has no validation
        var response = await behavior.Handle(request, Next, CancellationToken.None);
        Assert.NotEqual(Guid.Empty, response);
    }
}

/// <summary>
/// Test: ErrorHandlingBehavior extensible placeholder.
/// </summary>
public class ErrorHandlingBehaviorTests
{
    [Fact]
    public async Task ErrorHandlingBehavior_PassesThroughToNextHandler()
    {
        // Arrange
        var behavior = new ErrorHandlingBehavior<SaveTransactionCommand, Guid>();
        var request = new SaveTransactionCommand("Test", DateTime.UtcNow, 100m);
        var handlerExecuted = false;

        // Act
        async Task<Guid> Next()
        {
            handlerExecuted = true;
            return Guid.NewGuid();
        }

        var response = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        Assert.True(handlerExecuted);
        Assert.NotEqual(Guid.Empty, response);
    }

    [Fact]
    public async Task ErrorHandlingBehavior_CatchesAndRethrowsExceptions()
    {
        // Arrange
        var behavior = new ErrorHandlingBehavior<SaveTransactionCommand, Guid>();
        var request = new SaveTransactionCommand("Test", DateTime.UtcNow, 100m);
        var testException = new InvalidOperationException("Handler error");

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
    public async Task ErrorHandlingBehavior_IsPlaceholderForPhase2C()
    {
        // Arrange
        var behavior = new ErrorHandlingBehavior<SaveTransactionCommand, Guid>();

        // Act & Assert - Verify it's a placeholder (catches and re-throws)
        // Phase 2C will map exceptions

        async Task<Guid> ThrowingNext()
        {
            throw new ApplicationException("Domain error");
        }

        // Should re-throw because Phase 2B has no exception mapping
        await Assert.ThrowsAsync<ApplicationException>(
            () => behavior.Handle(
                new SaveTransactionCommand("Test", DateTime.UtcNow, 100m),
                ThrowingNext,
                CancellationToken.None));
    }

    [Fact]
    public async Task ErrorHandlingBehavior_HandlesMultipleExceptionTypes()
    {
        // Arrange
        var behavior = new ErrorHandlingBehavior<SaveTransactionCommand, Guid>();

        // Test InvalidOperationException
        var invalidOpException = new InvalidOperationException("Invalid");
        async Task<Guid> FailingNextInvalidOp()
        {
            throw invalidOpException;
        }

        var ex1 = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(
                new SaveTransactionCommand("Test", DateTime.UtcNow, 100m),
                FailingNextInvalidOp,
                CancellationToken.None));
        Assert.Equal(invalidOpException.Message, ex1.Message);

        // Test ArgumentNullException
        var argNullException = new ArgumentNullException("Null arg");
        async Task<Guid> FailingNextArgNull()
        {
            throw argNullException;
        }

        var ex2 = await Assert.ThrowsAsync<ArgumentNullException>(
            () => behavior.Handle(
                new SaveTransactionCommand("Test", DateTime.UtcNow, 100m),
                FailingNextArgNull,
                CancellationToken.None));
        Assert.Equal(argNullException.Message, ex2.Message);

        // Test generic Exception
        var genericException = new Exception("Generic error");
        async Task<Guid> FailingNextGeneric()
        {
            throw genericException;
        }

        var ex3 = await Assert.ThrowsAsync<Exception>(
            () => behavior.Handle(
                new SaveTransactionCommand("Test", DateTime.UtcNow, 100m),
                FailingNextGeneric,
                CancellationToken.None));
        Assert.Equal(genericException.Message, ex3.Message);
    }
}
