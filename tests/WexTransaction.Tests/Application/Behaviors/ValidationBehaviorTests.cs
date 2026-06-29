namespace WexTransaction.Tests.Application.Behaviors;

using FluentValidation;
using Moq;
using WexTransaction.Application.Behaviors;
using WexTransaction.Application.UseCases.SavePurchaseTransaction;

public class ValidationBehaviorTests
{
    private readonly List<IValidator<SaveTransactionCommand>> _validators;
    private readonly ValidationBehaviour<SaveTransactionCommand, Guid> _behavior;

    public ValidationBehaviorTests()
    {
        _validators = new List<IValidator<SaveTransactionCommand>>();
        _behavior = new ValidationBehaviour<SaveTransactionCommand, Guid>(_validators);
    }

    [Fact]
    public async Task ValidationBehavior_ValidRequest_CallsNext()
    {
        // Arrange
        var command = new SaveTransactionCommand("Valid Description", DateTime.UtcNow, 100m);
        var mockValidator = new Mock<IValidator<SaveTransactionCommand>>();
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SaveTransactionCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _validators.Add(mockValidator.Object);

        var nextCalled = false;
        Task<Guid> Next()
        {
            nextCalled = true;
            return Task.FromResult(Guid.NewGuid());
        }

        // Act
        await _behavior.Handle(command, Next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ValidationBehavior_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var command = new SaveTransactionCommand("", DateTime.UtcNow, 0m);
        var mockValidator = new Mock<IValidator<SaveTransactionCommand>>();
        var validationResult = new FluentValidation.Results.ValidationResult(new[]
        {
            new FluentValidation.Results.ValidationFailure("Description", "Description is required")
        });
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SaveTransactionCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);
        _validators.Add(mockValidator.Object);

        Task<Guid> Next() => Task.FromResult(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _behavior.Handle(command, Next, CancellationToken.None));
    }

    [Fact]
    public async Task ValidationBehavior_MultipleValidators_CollectsAllErrors()
    {
        // Arrange
        var command = new SaveTransactionCommand("", DateTime.UtcNow, 0m);

        var validator1 = new Mock<IValidator<SaveTransactionCommand>>();
        var result1 = new FluentValidation.Results.ValidationResult(new[]
        {
            new FluentValidation.Results.ValidationFailure("Description", "Description is required")
        });
        validator1
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SaveTransactionCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result1);

        var validator2 = new Mock<IValidator<SaveTransactionCommand>>();
        var result2 = new FluentValidation.Results.ValidationResult(new[]
        {
            new FluentValidation.Results.ValidationFailure("Amount", "Amount must be greater than 0")
        });
        validator2
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SaveTransactionCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result2);

        _validators.Add(validator1.Object);
        _validators.Add(validator2.Object);

        Task<Guid> Next() => Task.FromResult(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _behavior.Handle(command, Next, CancellationToken.None));
    }

    [Fact]
    public async Task ValidationBehavior_EmptyValidatorList_PassesToNext()
    {
        // Arrange
        var command = new SaveTransactionCommand("Valid", DateTime.UtcNow, 100m);
        var nextCalled = false;

        Task<Guid> Next()
        {
            nextCalled = true;
            return Task.FromResult(Guid.NewGuid());
        }

        // Act
        var result = await _behavior.Handle(command, Next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.NotEqual(Guid.Empty, result);
    }
}
