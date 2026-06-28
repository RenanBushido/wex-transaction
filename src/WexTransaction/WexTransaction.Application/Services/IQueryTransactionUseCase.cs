namespace WexTransaction.Application.Services;

public interface IQueryTransactionUseCase
{
    Task<QueryTransactionResponse?> ExecuteAsync(Guid transactionId, string country, string currency, CancellationToken cancellationToken = default);
}
