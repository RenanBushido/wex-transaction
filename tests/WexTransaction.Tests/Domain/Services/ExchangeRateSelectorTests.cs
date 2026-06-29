namespace WexTransaction.Tests.Domain.Services;

public class ExchangeRateSelectorTests
{
    private static readonly DateTime _purchaseDate = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    private static PurchaseTransaction CreateTransaction(decimal amount = 200.24m, DateTime? date = null) =>
        PurchaseTransaction.Create("Test transaction", date ?? _purchaseDate, amount);

    private static ExchangeRate Rate(decimal rate, DateTimeOffset effectiveDate) =>
        new("Brazil", "Real", rate, effectiveDate);

    [Fact]
    public void Convert_SingleQualifyingRate_ReturnsResult()
    {
        var transaction = CreateTransaction();
        var rates = new[] { Rate(5.25m, _purchaseDate.AddDays(-10)) };

        var result = ExchangeRateSelector.Convert(transaction, rates);

        Assert.Equal(5.25m, result.ExchangeRateUsed);
    }

    [Fact]
    public void Convert_MultipleQualifyingRates_SelectsMostRecent()
    {
        var transaction = CreateTransaction();
        var rates = new[]
        {
            Rate(4.00m, _purchaseDate.AddDays(-60)),
            Rate(5.25m, _purchaseDate.AddDays(-10)),
            Rate(3.00m, _purchaseDate.AddDays(-90))
        };

        var result = ExchangeRateSelector.Convert(transaction, rates);

        Assert.Equal(5.25m, result.ExchangeRateUsed);
    }

    [Fact]
    public void Convert_RateOnPurchaseDate_IsAccepted()
    {
        var transaction = CreateTransaction();
        var rates = new[] { Rate(5.25m, _purchaseDate) };

        var result = ExchangeRateSelector.Convert(transaction, rates);

        Assert.Equal(5.25m, result.ExchangeRateUsed);
    }

    [Fact]
    public void Convert_RateExactly6MonthsBefore_IsAccepted()
    {
        var transaction = CreateTransaction();
        var rates = new[] { Rate(5.25m, _purchaseDate.AddMonths(-6)) };

        var result = ExchangeRateSelector.Convert(transaction, rates);

        Assert.Equal(5.25m, result.ExchangeRateUsed);
    }

    [Fact]
    public void Convert_RateOneDayInsideWindow_IsAccepted()
    {
        var transaction = CreateTransaction();
        var rates = new[] { Rate(5.25m, _purchaseDate.AddMonths(-6).AddDays(1)) };

        var result = ExchangeRateSelector.Convert(transaction, rates);

        Assert.Equal(5.25m, result.ExchangeRateUsed);
    }

    [Fact]
    public void Convert_RateAfterPurchaseDate_ThrowsCurrencyConversionUnavailableException()
    {
        var transaction = CreateTransaction();
        var rates = new[] { Rate(5.25m, _purchaseDate.AddDays(1)) };

        Assert.Throws<CurrencyConversionUnavailableException>(() =>
            ExchangeRateSelector.Convert(transaction, rates));
    }

    [Fact]
    public void Convert_RateOlderThan6Months_ThrowsCurrencyConversionUnavailableException()
    {
        var transaction = CreateTransaction();
        var rates = new[] { Rate(5.25m, _purchaseDate.AddMonths(-6).AddDays(-1)) };

        Assert.Throws<CurrencyConversionUnavailableException>(() =>
            ExchangeRateSelector.Convert(transaction, rates));
    }

    [Fact]
    public void Convert_EmptyRateList_ThrowsCurrencyConversionUnavailableException()
    {
        var transaction = CreateTransaction();

        Assert.Throws<CurrencyConversionUnavailableException>(() =>
            ExchangeRateSelector.Convert(transaction, Array.Empty<ExchangeRate>()));
    }

    [Fact]
    public void Convert_OnlyOutOfWindowRates_ThrowsCurrencyConversionUnavailableException()
    {
        var transaction = CreateTransaction();
        var rates = new[]
        {
            Rate(5.25m, _purchaseDate.AddDays(5)),
            Rate(4.00m, _purchaseDate.AddMonths(-7))
        };

        Assert.Throws<CurrencyConversionUnavailableException>(() =>
            ExchangeRateSelector.Convert(transaction, rates));
    }

    [Fact]
    public void Convert_ResultFieldsArePopulatedCorrectly()
    {
        var transaction = CreateTransaction();
        var rates = new[] { Rate(5.25m, _purchaseDate.AddDays(-10)) };

        var result = ExchangeRateSelector.Convert(transaction, rates);

        Assert.Equal(transaction.Id, result.TransactionId);
        Assert.Equal("Test transaction", result.Description);
        Assert.Equal(_purchaseDate, result.TransactionDate);
        Assert.Equal(200.24m, result.OriginalAmountUsd);
        Assert.Equal(5.25m, result.ExchangeRateUsed);
    }

    [Fact]
    public void Convert_MidpointRoundsAwayFromZero()
    {
        // 2.00 * 1.0025 = 2.005 → MidpointRounding.AwayFromZero → 2.01
        var transaction = CreateTransaction(amount: 2.00m);
        var rates = new[] { Rate(1.0025m, _purchaseDate.AddDays(-1)) };

        var result = ExchangeRateSelector.Convert(transaction, rates);

        Assert.Equal(2.01m, result.ConvertedAmount);
    }
}
