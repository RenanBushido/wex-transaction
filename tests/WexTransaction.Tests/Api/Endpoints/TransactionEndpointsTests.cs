namespace WexTransaction.Tests.Api.Endpoints;

public class TransactionEndpointsTests
{
    private readonly Mock<IMediator> _mockMediator;

    private static readonly DateTime ValidDate = new DateTime(2026, 6, 23, 8, 30, 0, DateTimeKind.Utc);
    private const string ValidDescription = "Coffee";
    private const decimal ValidAmount = 100.50m;

    public TransactionEndpointsTests()
    {
        _mockMediator = new Mock<IMediator>();
    }

    [Fact]
    public async Task SaveTransaction_WithValidRequest_ReturnsCreatedWith201()
    {
        // Arrange
        var request = new SaveTransactionRequest(ValidDescription, ValidDate, ValidAmount);
        var expectedId = Guid.NewGuid();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<SaveTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await WexTransaction.Api.Endpoints.SaveTransaction(request, _mockMediator.Object, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mockMediator.Verify(
            m => m.Send(It.IsAny<SaveTransactionCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveTransaction_WithValidInput_DispatchesCorrectCommand()
    {
        // Arrange
        var request = new SaveTransactionRequest(ValidDescription, ValidDate, ValidAmount);
        _mockMediator
            .Setup(m => m.Send(It.IsAny<SaveTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await WexTransaction.Api.Endpoints.SaveTransaction(request, _mockMediator.Object, CancellationToken.None);

        // Assert
        _mockMediator.Verify(
            m => m.Send(
                It.Is<SaveTransactionCommand>(cmd =>
                    cmd.Description == ValidDescription &&
                    cmd.Date == ValidDate &&
                    cmd.Amount == ValidAmount),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
