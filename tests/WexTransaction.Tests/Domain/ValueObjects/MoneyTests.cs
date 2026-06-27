using WexTransaction.Domain.Exceptions;
using WexTransaction.Domain.ValueObjects;

namespace WexTransaction.Tests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithPositiveAmount_StoresValue()
    {
        var money = new Money(100.00m);
        Assert.Equal(100.00m, money.Value);
    }

    [Fact]
    public void Create_WithAmountRequiringRounding_RoundsToNearestCent()
    {
        var money = new Money(200.245m);
        Assert.Equal(200.25m, money.Value);
    }

    [Fact]
    public void Create_WithMidpointAmount_RoundsAwayFromZero()
    {
        var money = new Money(1.005m);
        Assert.Equal(1.01m, money.Value);
    }

    [Fact]
    public void Create_WithAmountRoundingDown_RoundsCorrectly()
    {
        var money = new Money(200.244m);
        Assert.Equal(200.24m, money.Value);
    }

    [Fact]
    public void Create_WithZeroAmount_ThrowsInvalidAmountException()
    {
        Assert.Throws<InvalidAmountException>(() => new Money(0m));
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsInvalidAmountException()
    {
        Assert.Throws<InvalidAmountException>(() => new Money(-10.00m));
    }

    [Fact]
    public void ImplicitConversion_ToDecimal_ReturnsValue()
    {
        var money = new Money(50.00m);
        decimal result = money;
        Assert.Equal(50.00m, result);
    }
}
