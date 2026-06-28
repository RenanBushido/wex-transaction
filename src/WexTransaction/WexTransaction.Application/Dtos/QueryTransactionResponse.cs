namespace WexTransaction.Application.Dtos;

public record QueryTransactionResponse(
    Guid TransactionId,
    string Description,
    DateTime Date,
    decimal Amount,
    decimal TaxRate,
    decimal ConvertedValue
);
