namespace WexTransaction.Application.Services;

public class GetTransactionUseCase : IQueryTransactionUseCase
{
    private readonly IMediator _mediator;

    public GetTransactionUseCase(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<QueryTransactionResponse?> ExecuteAsync(Guid transactionId, string country, string currency, CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionQuery(transactionId, country, currency);
        return await _mediator.Send(query, cancellationToken);
    }
}
