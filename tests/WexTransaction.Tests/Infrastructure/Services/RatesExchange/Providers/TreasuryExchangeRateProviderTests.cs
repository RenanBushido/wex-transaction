namespace WexTransaction.Tests.Infrastructure.Services.RatesExchange.Providers;

public class TreasuryExchangeRateProviderTests
{
    private static readonly DateTimeOffset ValidDate = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    private static TreasuryExchangeRateProvider CreateProvider(Mock<ITreasuryExchangeRateClient> mockClient) =>
        new(mockClient.Object, new Mock<ILogger<TreasuryExchangeRateProvider>>().Object);

    [Fact]
    public async Task GetExchangeRatesAsync_WithValidResponse_ReturnsMappedExchangeRates()
    {
        var mockClient = new Mock<ITreasuryExchangeRateClient>();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country_Currency_Desc = "Brazil-Real",
                    Exchange_Rate = 5.25m,
                    Effective_Date = ValidDate.ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = CreateProvider(mockClient);

        var result = await provider.GetExchangeRatesAsync("2026-06-23", "Brazil", "Real");

        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.Equal("Brazil", result.First().Country);
        Assert.Equal("Real", result.First().Currency);
        Assert.Equal(5.25m, result.First().Rate);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithMultipleRates_ReturnsAllMappedRates()
    {
        var mockClient = new Mock<ITreasuryExchangeRateClient>();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country_Currency_Desc = "Brazil-Real",
                    Exchange_Rate = 5.25m,
                    Effective_Date = ValidDate.ToString("O")
                },
                new()
                {
                    Country_Currency_Desc = "Brazil-Real",
                    Exchange_Rate = 5.20m,
                    Effective_Date = ValidDate.AddDays(-1).ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = CreateProvider(mockClient);

        var result = await provider.GetExchangeRatesAsync("2026-06-23", "Brazil", "Real");

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithEmptyResponse_ThrowsCurrencyConversionUnavailableException()
    {
        var mockClient = new Mock<ITreasuryExchangeRateClient>();
        mockClient
            .Setup(c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()))
            .ReturnsAsync(new TreasuryRatesResponse { Data = new List<TreasuryRateData>() });

        var provider = CreateProvider(mockClient);

        await Assert.ThrowsAsync<CurrencyConversionUnavailableException>(
            () => provider.GetExchangeRatesAsync("2026-06-23", "Brazil", "Real"));
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithNullResponse_ThrowsCurrencyConversionUnavailableException()
    {
        var mockClient = new Mock<ITreasuryExchangeRateClient>();
        mockClient
            .Setup(c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()))
            .ReturnsAsync(new TreasuryRatesResponse { Data = null });

        var provider = CreateProvider(mockClient);

        await Assert.ThrowsAsync<CurrencyConversionUnavailableException>(
            () => provider.GetExchangeRatesAsync("2026-06-23", "Brazil", "Real"));
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithHttpRequestException_ThrowsCurrencyConversionUnavailableException()
    {
        var mockClient = new Mock<ITreasuryExchangeRateClient>();
        mockClient
            .Setup(c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var provider = CreateProvider(mockClient);

        await Assert.ThrowsAsync<CurrencyConversionUnavailableException>(
            () => provider.GetExchangeRatesAsync("2026-06-23", "Brazil", "Real"));
    }

    [Fact]
    public async Task GetExchangeRatesAsync_CallsClientWithFilterContainingCountryAndCurrency()
    {
        var mockClient = new Mock<ITreasuryExchangeRateClient>();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country_Currency_Desc = "Brazil-Real",
                    Exchange_Rate = 5.25m,
                    Effective_Date = ValidDate.ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = CreateProvider(mockClient);

        await provider.GetExchangeRatesAsync("2026-06-23", "Brazil", "Real");

        mockClient.Verify(
            c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.Is<string?>(filter => filter != null && filter.Contains("Brazil") && filter.Contains("Real")),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_CachesSameRequest()
    {
        var mockClient = new Mock<ITreasuryExchangeRateClient>();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country_Currency_Desc = "Brazil-Real",
                    Exchange_Rate = 5.25m,
                    Effective_Date = ValidDate.ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = CreateProvider(mockClient);

        await provider.GetExchangeRatesAsync("2026-06-23", "Brazil", "Real");
        await provider.GetExchangeRatesAsync("2026-06-23", "Brazil", "Real");

        mockClient.Verify(
            c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithNullCountryCurrencyDesc_FiltersOutAndThrows()
    {
        var mockClient = new Mock<ITreasuryExchangeRateClient>();
        var response = new TreasuryRatesResponse
        {
            Data = new List<TreasuryRateData>
            {
                new()
                {
                    Country_Currency_Desc = null,
                    Exchange_Rate = 5.25m,
                    Effective_Date = ValidDate.ToString("O")
                }
            }
        };

        mockClient
            .Setup(c => c.GetExchangeRatesAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>()))
            .ReturnsAsync(response);

        var provider = CreateProvider(mockClient);

        await Assert.ThrowsAsync<CurrencyConversionUnavailableException>(
            () => provider.GetExchangeRatesAsync("2026-06-23", "Brazil", "Real"));
    }
}
