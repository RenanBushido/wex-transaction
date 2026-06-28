namespace WexTransaction.Application.UseCases.SavePurchaseTransaction;

public sealed record SaveTransactionCommand(
    string Description,
    DateTime Date,
    decimal Amount
) : IRequest<Guid>;
