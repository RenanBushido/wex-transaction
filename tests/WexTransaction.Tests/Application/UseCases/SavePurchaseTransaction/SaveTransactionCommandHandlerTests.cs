namespace WexTransaction.Tests.Application.UseCases.SavePurchaseTransaction;

public class SaveTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly SaveTransactionCommandHandler _handler;

    private static readonly DateTime ValidDate = new DateTime(2026, 6, 23, 8, 30, 0, DateTimeKind.Utc);
    private const string ValidDescription = "Coffee purchase";
    private const decimal ValidAmount = 200.24m;

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
        var command = new SaveTransactionCommand(ValidDescription, ValidDate, ValidAmount);

        // Mock setup
        _mockRepository
            .Setup(r => r.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.Commit(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _mockRepository.Verify(
            r => r.SavePurchaseTransaction(It.IsAny<PurchaseTransaction>()),
            Times.Once);
        _mockUnitOfWork.Verify(
            u => u.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
