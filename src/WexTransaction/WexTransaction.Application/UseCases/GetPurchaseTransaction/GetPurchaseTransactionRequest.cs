namespace WexTransaction.Application.UseCases.GetPurchaseTransaction;

public sealed record GetPurchaseTransactionRequest(
    Guid TransactionId,
    string Country,
    string Currency): IRequest<GetPurchaseTransactionResponse>;