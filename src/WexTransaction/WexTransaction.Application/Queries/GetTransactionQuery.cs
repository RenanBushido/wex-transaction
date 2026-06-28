namespace WexTransaction.Application.Queries;

public record GetTransactionQuery(
    Guid TransactionId,
    string Country,
    string Currency
) : IRequest<QueryTransactionResponse?>;
