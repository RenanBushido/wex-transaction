namespace WexTransaction.Application.UseCases.GetPurchaseTransaction;

public record GetPurchaseTransactionResponse(
    Guid TransactionId,
    string Description,
    DateTime Date,
    string Amount,
    string TaxRate,
    string ConvertedValue
);