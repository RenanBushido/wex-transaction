namespace WexTransaction.Application.PurchaseTransaction.GetByTransactionId;

using WexTransaction.Domain.Interfaces;

/// <summary>
/// Query Handler: Recupera transação com conversão de moeda.
/// Orquestração: Delegada para ITransactionQueryService (Domain port implementation).
/// Responsabilidade: Mapear query para resultado de negócio via serviço.
/// </summary>
public class GetTransactionIdQueryHandler(
    ITransactionQueryService queryService) : IRequestHandler<GetTransactionIdQuery, QueryTransactionResponse?>
{
    private readonly ITransactionQueryService _queryService = queryService;

    public async Task<QueryTransactionResponse?> Handle(
        GetTransactionIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetTransactionWithConversionAsync(
            request.TransactionId,
            request.Country,
            request.Currency,
            cancellationToken);

        if (result == null)
            return null;

        // Map from domain query result to DTO
        return new QueryTransactionResponse(
            TransactionId: result.TransactionId,
            Description: result.Description,
            Date: result.Date,
            Amount: result.Amount,
            TaxRate: result.TaxRate,
            ConvertedValue: result.ConvertedValue);

        // TODO: Phase 2B - publish TransactionConvertedEvent via injected IEventPublisher (deferred)
    }
}
