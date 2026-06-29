namespace WexTransaction.Domain.Entities;

public sealed class PurchaseTransaction : BaseAuditableEntity
{
    public TransactionDescription Description { get; private set; }
    public DateTime TransactionDate { get; private set; }
    // public Money Amount { get; private set; }
    public decimal Amount { get; private set; }

    private PurchaseTransaction() { }

    public static PurchaseTransaction Create(
        string description,
        DateTime transactionDate,
        decimal amount)
    {
        if (transactionDate == default)
            throw new InvalidTransactionDateException("Transaction date must not be a default value.");

        if (amount <= 0)
            throw new InvalidAmountException("Amount must be greater than zero.");

        return new PurchaseTransaction
        {
            Id = Guid.NewGuid(),
            Description = new TransactionDescription(description),
            TransactionDate = transactionDate,
            Amount = amount,
            CreatedAt = DateTime.UtcNow
        };
    }
}
