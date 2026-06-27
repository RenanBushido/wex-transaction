using WexTransaction.Domain.ValueObjects;

namespace WexTransaction.Tests.Domain.ValueObjects;

public class ConvertedTransactionResultTests
{
    [Fact]
    public void Constructor_SetsAllFields()
    {
        var id = Guid.NewGuid();
        var date = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

        var result = new ConvertedTransactionResult(id, "Test", date, 100m, 5.25m, 525m);

        Assert.Equal(id, result.TransactionId);
        Assert.Equal("Test", result.Description);
        Assert.Equal(date, result.TransactionDate);
        Assert.Equal(100m, result.OriginalAmountUsd);
        Assert.Equal(5.25m, result.ExchangeRateUsed);
        Assert.Equal(525m, result.ConvertedAmount);
    }

    [Fact]
    public void TwoInstancesWithSameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var date = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

        var r1 = new ConvertedTransactionResult(id, "Test", date, 100m, 5.25m, 525m);
        var r2 = new ConvertedTransactionResult(id, "Test", date, 100m, 5.25m, 525m);

        Assert.Equal(r1, r2);
    }
}
