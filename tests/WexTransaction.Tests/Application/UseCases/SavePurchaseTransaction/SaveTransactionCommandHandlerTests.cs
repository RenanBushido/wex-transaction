namespace WexTransaction.Tests.Application.UseCases.SavePurchaseTransaction;

using Moq;
using Xunit;
using WexTransaction.Application.UseCases.SavePurchaseTransaction;
using WexTransaction.Domain.Entities;
using WexTransaction.Domain.Interfaces;

/// <summary>
/// Test: SaveTransactionCommandHandler - Creates and saves purchase transactions
/// </summary>
public class SaveTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly SaveTransactionCommandHandler _handler;

    public SaveTransactionCommandHandlerTests()
    {
        _mockRepository = new Mock<ITransactionRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new SaveTransactionCommandHandler(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_SavesTransactionAndReturnsId()
    {
        // Arrange
        var command = new SaveTransactionCommand(
            Description: "Test Transaction",
            Date: DateTime.UtcNow,
            Amount: 100.50m
        );

        _mockUnitOfWork
            .Setup(x => x.Commit(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _mockRepository.Verify(x => x.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsRepositoryBeforeUnitOfWork()
    {
        // Arrange
        var command = new SaveTransactionCommand(
            Description: "Test Transaction",
            Date: DateTime.UtcNow,
            Amount: 100.50m
        );

        var callOrder = new List<string>();

        _mockRepository
            .Setup(x => x.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()))
            .Callback(() => callOrder.Add("Repository"))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.Commit(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("UnitOfWork"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, callOrder.Count);
        Assert.Equal("Repository", callOrder[0]);
        Assert.Equal("UnitOfWork", callOrder[1]);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesTransactionWithCorrectData()
    {
        // Arrange
        var description = "Test Description";
        var date = DateTime.UtcNow;
        var amount = 250.75m;

        var command = new SaveTransactionCommand(
            Description: description,
            Date: date,
            Amount: amount
        );

        PurchaseTransaction? capturedTransaction = null;

        _mockRepository
            .Setup(x => x.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()))
            .Callback<PurchaseTransaction>(t => capturedTransaction = t)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.Commit(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedTransaction);
        Assert.Equal(description, (string)capturedTransaction.Description);
        Assert.Equal(date, capturedTransaction.TransactionDate);
        Assert.Equal(amount, (decimal)capturedTransaction.Amount);
        Assert.Equal(capturedTransaction.Id, result);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var command = new SaveTransactionCommand(
            Description: "Test Transaction",
            Date: DateTime.UtcNow,
            Amount: 100.50m
        );

        var testException = new InvalidOperationException("Repository error");

        _mockRepository
            .Setup(x => x.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()))
            .ThrowsAsync(testException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(testException.Message, exception.Message);
        _mockUnitOfWork.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkThrows_PropagatesException()
    {
        // Arrange
        var command = new SaveTransactionCommand(
            Description: "Test Transaction",
            Date: DateTime.UtcNow,
            Amount: 100.50m
        );

        var testException = new InvalidOperationException("UnitOfWork error");

        _mockRepository
            .Setup(x => x.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.Commit(It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(testException.Message, exception.Message);
    }

    [Fact]
    public async Task Handle_ReturnsUniqueIdForEachTransaction()
    {
        // Arrange
        var command1 = new SaveTransactionCommand("Test 1", DateTime.UtcNow, 100m);
        var command2 = new SaveTransactionCommand("Test 2", DateTime.UtcNow, 200m);

        _mockUnitOfWork
            .Setup(x => x.Commit(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result1);
        Assert.NotEqual(Guid.Empty, result2);
        Assert.NotEqual(result1, result2);
    }
}
