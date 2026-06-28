namespace WexTransaction.Application.Services;

public interface ICreateTransactionUseCase
{
    Task<Guid> ExecuteAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
}
