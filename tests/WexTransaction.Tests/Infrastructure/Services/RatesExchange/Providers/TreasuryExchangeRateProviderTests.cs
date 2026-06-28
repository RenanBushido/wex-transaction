namespace WexTransaction.Tests.Infrastructure.Services.RatesExchange.Providers;
public class TreasuryExchangeRateProviderTests
{
    private static readonly DateTimeOffset ValidDate = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    private Mock<ITreasuryExchangeRateClient> CreateMockClient()
    {
        return new Mock<ITreasuryExchangeRateClient>();
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithValidResponse_ReturnsMappedExchangeRates()
    {
        var mockClient = CreateMockClient();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country = "Brazil",
                    Currency = "Real",
                    ExchangeRate = 5.25m,
                    EffectiveDate = ValidDate.ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = new TreasuryExchangeRateProvider(mockClient.Object);

        var result = await provider.GetExchangeRatesAsync("Brazil", "Real");

        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.Equal("Brazil", result.First().Country);
        Assert.Equal("Real", result.First().Currency);
        Assert.Equal(5.25m, result.First().Rate);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithMultipleRates_ReturnsAllMappedRates()
    {
        var mockClient = CreateMockClient();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country = "Brazil",
                    Currency = "Real",
                    ExchangeRate = 5.25m,
                    EffectiveDate = ValidDate.ToString("O")
                },
                new()
                {
                    Country = "Brazil",
                    Currency = "Real",
                    ExchangeRate = 5.20m,
                    EffectiveDate = ValidDate.AddDays(-1).ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = new TreasuryExchangeRateProvider(mockClient.Object);

        var result = await provider.GetExchangeRatesAsync("Brazil", "Real");

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithEmptyResponse_ThrowsCurrencyConversionUnavailableException()
    {
        var mockClient = CreateMockClient();
        var response = new TreasuryRatesResponse { Data = new List<TreasuryRateData>() };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = new TreasuryExchangeRateProvider(mockClient.Object);

        await Assert.ThrowsAsync<CurrencyConversionUnavailableException>(
            () => provider.GetExchangeRatesAsync("Brazil", "Real"));
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithNullResponse_ThrowsCurrencyConversionUnavailableException()
    {
        var mockClient = CreateMockClient();
        mockClient
            .Setup(c => c.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new TreasuryRatesResponse { Data = null });

        var provider = new TreasuryExchangeRateProvider(mockClient.Object);

        await Assert.ThrowsAsync<CurrencyConversionUnavailableException>(
            () => provider.GetExchangeRatesAsync("Brazil", "Real"));
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithHttpRequestException_ThrowsCurrencyConversionUnavailableException()
    {
        var mockClient = CreateMockClient();
        mockClient
            .Setup(c => c.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var provider = new TreasuryExchangeRateProvider(mockClient.Object);

        await Assert.ThrowsAsync<CurrencyConversionUnavailableException>(
            () => provider.GetExchangeRatesAsync("Brazil", "Real"));
    }

    [Fact]
    public async Task GetExchangeRatesAsync_CallsClientWithCorrectParameters()
    {
        var mockClient = CreateMockClient();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country = "Brazil",
                    Currency = "Real",
                    ExchangeRate = 5.25m,
                    EffectiveDate = ValidDate.ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = new TreasuryExchangeRateProvider(mockClient.Object);

        await provider.GetExchangeRatesAsync("Brazil", "Real");

        mockClient.Verify(
            c => c.GetExchangeRatesAsync(
                It.Is<string>(s => s.Contains("Brazil") && s.Contains("Real")),
                It.IsAny<string>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_CachesSameRequest()
    {
        var mockClient = CreateMockClient();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country = "Brazil",
                    Currency = "Real",
                    ExchangeRate = 5.25m,
                    EffectiveDate = ValidDate.ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = new TreasuryExchangeRateProvider(mockClient.Object);

        await provider.GetExchangeRatesAsync("Brazil", "Real");
        await provider.GetExchangeRatesAsync("Brazil", "Real");

        mockClient.Verify(
            c => c.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithInvalidData_FiltersOutAndThrowsIfEmpty()
    {
        var mockClient = CreateMockClient();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country = null,
                    Currency = "Real",
                    ExchangeRate = 5.25m,
                    EffectiveDate = ValidDate.ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = new TreasuryExchangeRateProvider(mockClient.Object);

        await Assert.ThrowsAsync<CurrencyConversionUnavailableException>(
            () => provider.GetExchangeRatesAsync("Brazil", "Real"));
    }
}
