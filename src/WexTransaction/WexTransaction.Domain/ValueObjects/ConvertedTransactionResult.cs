namespace WexTransaction.Domain.ValueObjects;

public sealed record ConvertedTransactionResult(
    Guid TransactionId,
    string Description,
    DateTimeOffset TransactionDate,
    decimal OriginalAmountUsd,
    decimal ExchangeRateUsed,
    decimal ConvertedAmount);
