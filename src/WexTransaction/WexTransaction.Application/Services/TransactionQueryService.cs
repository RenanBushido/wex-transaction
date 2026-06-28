using WexTransaction.Domain.Interfaces;
using WexTransaction.Domain.Services;

namespace WexTransaction.Application.Services;

/// <summary>
/// Implementação do port ITransactionQueryService.
/// Orquestra a consulta de transações com conversão de moeda.
/// Responsabilidades:
/// 1. Recuperar transação do repositório
/// 2. Buscar taxas de câmbio do provedor externo
/// 3. Calcular valor convertido
/// 4. Mapear resultado para DTO
/// </summary>
public class TransactionQueryService(
    ITransactionRepository repository,
    IExchangeRateProvider exchangeRateProvider,
    IMapper mapper) : ITransactionQueryService
{
    private readonly ITransactionRepository _repository = repository;
    private readonly IExchangeRateProvider _exchangeRateProvider = exchangeRateProvider;
    private readonly IMapper _mapper = mapper;

    public async Task<TransactionQueryResult?> GetTransactionWithConversionAsync(
        Guid transactionId,
        string country,
        string currency,
        CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByIdAsync(transactionId);
        if (transaction == null)
            return null;

        var exchangeRates = await _exchangeRateProvider.GetExchangeRatesAsync(country, currency);
        var convertedResult = ExchangeRateSelector.Convert(transaction, exchangeRates);

        var response = _mapper.Map<QueryTransactionResponse>(transaction);
        var converted = response with
        {
            TaxRate = convertedResult.ExchangeRateUsed,
            ConvertedValue = convertedResult.ConvertedAmount
        };

        return new TransactionQueryResult(
            TransactionId: converted.TransactionId,
            Description: converted.Description,
            Date: converted.Date,
            Amount: converted.Amount,
            TaxRate: converted.TaxRate,
            ConvertedValue: converted.ConvertedValue);
    }
}
