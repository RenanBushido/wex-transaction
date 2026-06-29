namespace WexTransaction.Tests.Application.UseCases.GetPurchaseTransaction;

public class GetPurchaseTransactionHandlerTests
{
    private readonly Mock<ITransactionDapperRepository> _mockRepository;
    private readonly Mock<IExchangeRateProvider> _mockExchangeRateProvider;
    private readonly GetPurchaseTransactionHandler _handler;

    private static readonly DateTime TransactionDate = new DateTime(2026, 6, 23, 8, 30, 0, DateTimeKind.Utc);
    private static readonly Guid TransactionId = Guid.NewGuid();
    private const string Country = "Brazil";
    private const string Currency = "BRL";

    public GetPurchaseTransactionHandlerTests()
    {
        _mockRepository = new Mock<ITransactionDapperRepository>();
        _mockExchangeRateProvider = new Mock<IExchangeRateProvider>();
        _handler = new GetPurchaseTransactionHandler(
            _mockRepository.Object,
            _mockExchangeRateProvider.Object,
            new Mock<ILogger<GetPurchaseTransactionHandler>>().Object);
    }

    [Fact]
    public async Task Handle_WithValidRequestAndExistingTransaction_ReturnsConvertedResponse()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Coffee", TransactionDate, 100m);
        var exchangeRates = new List<ExchangeRate>
        {
            new ExchangeRate(Country, Currency, 5.25m, new DateTimeOffset(TransactionDate))
        };

        var request = new GetPurchaseTransactionRequest(transaction.Id, Country, Currency);

        _mockRepository
            .Setup(r => r.GetByIdAsync(transaction.Id))
            .ReturnsAsync(transaction);

        _mockExchangeRateProvider
            .Setup(e => e.GetExchangeRatesAsync(TransactionDate.ToString("yyyy-MM-dd"), Country, Currency))
            .ReturnsAsync(exchangeRates);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.TransactionId);
        Assert.Equal((string)transaction.Description, result.Description);
        Assert.Equal((decimal)transaction.Amount, result.Amount);
        Assert.True(result.ConvertedValue > 0);

        _mockRepository.Verify(r => r.GetByIdAsync(transaction.Id), Times.Once);
        _mockExchangeRateProvider.Verify(e => e.GetExchangeRatesAsync(TransactionDate.ToString("yyyy-MM-dd"), Country, Currency), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTransactionId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new GetPurchaseTransactionRequest(nonExistentId, Country, Currency);

        _mockRepository
            .Setup(r => r.GetByIdAsync(nonExistentId))
            .ReturnsAsync((PurchaseTransaction)null!);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(nonExistentId), Times.Once);
        _mockExchangeRateProvider.Verify(e => e.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidTransactionAndExchangeRates_ConvertsAmountCorrectly()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Book", TransactionDate, 50m);
        var exchangeRate = 3.50m; // 1 USD = 3.50 BRL
        var exchangeRates = new List<ExchangeRate>
        {
            new ExchangeRate(Country, Currency, exchangeRate, new DateTimeOffset(TransactionDate))
        };

        var request = new GetPurchaseTransactionRequest(transaction.Id, Country, Currency);

        _mockRepository
            .Setup(r => r.GetByIdAsync(transaction.Id))
            .ReturnsAsync(transaction);

        _mockExchangeRateProvider
            .Setup(e => e.GetExchangeRatesAsync(TransactionDate.ToString("yyyy-MM-dd"), Country, Currency))
            .ReturnsAsync(exchangeRates);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        // Expected: 50m * 3.50m = 175m
        Assert.Equal(175m, result.ConvertedValue);
        Assert.Equal(exchangeRate, result.TaxRate);
    }
}
