using WexTransaction.Domain.Entities;
using WexTransaction.Domain.Exceptions;

namespace WexTransaction.Tests.Domain.Entities;

public class PurchaseTransactionTests
{
    private static readonly DateTimeOffset ValidDate = new DateTimeOffset(2026, 6, 23, 8, 30, 0, TimeSpan.Zero);
    private const string ValidDescription = "Coffee purchase";
    private const decimal ValidAmount = 200.24m;

    [Fact]
    public void Create_WithValidInputs_ReturnsFullyPopulatedEntity()
    {
        var transaction = PurchaseTransaction.Create(ValidDescription, ValidDate, ValidAmount);

        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(ValidDescription, transaction.Description.Value);
        Assert.Equal(ValidDate, transaction.TransactionDate);
        Assert.Equal(ValidAmount, transaction.Amount.Value);
    }

    [Fact]
    public void Create_TwiceWithValidInputs_ProducesDifferentIds()
    {
        var t1 = PurchaseTransaction.Create(ValidDescription, ValidDate, ValidAmount);
        var t2 = PurchaseTransaction.Create(ValidDescription, ValidDate, ValidAmount);

        Assert.NotEqual(t1.Id, t2.Id);
    }

    [Fact]
    public void Create_WithDefaultDate_ThrowsInvalidTransactionDateException()
    {
        Assert.Throws<InvalidTransactionDateException>(() =>
            PurchaseTransaction.Create(ValidDescription, default, ValidAmount));
    }

    [Fact]
    public void Create_WithNonUtcDate_ThrowsInvalidTransactionDateException()
    {
        var nonUtcDate = new DateTimeOffset(2026, 6, 23, 8, 30, 0, TimeSpan.FromHours(-3));

        Assert.Throws<InvalidTransactionDateException>(() =>
            PurchaseTransaction.Create(ValidDescription, nonUtcDate, ValidAmount));
    }

    [Fact]
    public void Create_WithEmptyDescription_ThrowsInvalidDescriptionException()
    {
        Assert.Throws<InvalidDescriptionException>(() =>
            PurchaseTransaction.Create(string.Empty, ValidDate, ValidAmount));
    }

    [Fact]
    public void Create_WithDescriptionOver50Chars_ThrowsInvalidDescriptionException()
    {
        var longDescription = new string('x', 51);

        Assert.Throws<InvalidDescriptionException>(() =>
            PurchaseTransaction.Create(longDescription, ValidDate, ValidAmount));
    }

    [Fact]
    public void Create_WithZeroAmount_ThrowsInvalidAmountException()
    {
        Assert.Throws<InvalidAmountException>(() =>
            PurchaseTransaction.Create(ValidDescription, ValidDate, 0m));
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsInvalidAmountException()
    {
        Assert.Throws<InvalidAmountException>(() =>
            PurchaseTransaction.Create(ValidDescription, ValidDate, -5.00m));
    }
}
