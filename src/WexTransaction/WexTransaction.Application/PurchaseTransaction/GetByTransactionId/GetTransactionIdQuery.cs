namespace WexTransaction.Application.PurchaseTransaction.GetByTransactionId;

public record GetTransactionIdQuery(
    Guid TransactionId,
    string Country,
    string Currency
) : IRequest<QueryTransactionResponse?>;
