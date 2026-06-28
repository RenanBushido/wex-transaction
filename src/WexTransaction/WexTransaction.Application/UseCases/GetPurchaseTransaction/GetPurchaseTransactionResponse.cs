namespace WexTransaction.Application.UseCases.GetPurchaseTransaction;

public record GetPurchaseTransactionResponse(
    Guid TransactionId,
    string Description,
    DateTime Date,
    decimal Amount,
    decimal TaxRate,
    decimal ConvertedValue
);