namespace WexTransaction.Tests.Domain.ValueObjects;

public class ExchangeRateTests
{
    private static readonly DateTimeOffset _validDate = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_ValidInputs_CreatesExchangeRate()
    {
        var rate = new ExchangeRate("Brazil", "Real", 5.25m, _validDate);

        Assert.Equal("Brazil", rate.Country);
        Assert.Equal("Real", rate.Currency);
        Assert.Equal(5.25m, rate.Rate);
        Assert.Equal(_validDate, rate.EffectiveDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Constructor_NonPositiveRate_ThrowsArgumentOutOfRangeException(decimal rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExchangeRate("Brazil", "Real", rate, _validDate));
    }

    [Fact]
    public void Constructor_NonUtcEffectiveDate_CreatesExchangeRate()
    {
        var nonUtcDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-3));

        var rate = new ExchangeRate("Brazil", "Real", 5.25m, nonUtcDate);

        Assert.Equal(nonUtcDate, rate.EffectiveDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrEmptyCountry_ThrowsArgumentException(string? country)
    {
        Assert.Throws<ArgumentException>(() => new ExchangeRate(country!, "Real", 5.25m, _validDate));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrEmptyCurrency_ThrowsArgumentException(string? currency)
    {
        Assert.Throws<ArgumentException>(() => new ExchangeRate("Brazil", currency!, 5.25m, _validDate));
    }

    [Fact]
    public void TwoInstancesWithSameValues_AreEqual()
    {
        var rate1 = new ExchangeRate("Brazil", "Real", 5.25m, _validDate);
        var rate2 = new ExchangeRate("Brazil", "Real", 5.25m, _validDate);

        Assert.Equal(rate1, rate2);
    }
}
