namespace WexTransaction.Domain.Entities;

public sealed class PurchaseTransaction : BaseAuditableEntity
{
    public TransactionDescription Description { get; private set; }
    public DateTimeOffset TransactionDate { get; private set; }
    public Money Amount { get; private set; }

    private PurchaseTransaction() { }

    public static PurchaseTransaction Create(
        string description,
        DateTimeOffset transactionDate,
        decimal amount)
    {
        if (transactionDate == default)
            throw new InvalidTransactionDateException("Transaction date must not be a default value.");

        if (transactionDate.Offset != TimeSpan.Zero)
            throw new InvalidTransactionDateException("Transaction date must be in UTC (offset must be zero).");

        return new PurchaseTransaction
        {
            Id = Guid.NewGuid(),
            Description = new TransactionDescription(description),
            TransactionDate = transactionDate,
            Amount = new Money(amount)
        };
    }
}
