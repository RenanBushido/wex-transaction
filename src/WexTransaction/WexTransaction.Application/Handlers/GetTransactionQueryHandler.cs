using WexTransaction.Application.Services;
using WexTransaction.Domain.Interfaces;
using WexTransaction.Domain.Services;

namespace WexTransaction.Application.Handlers;

public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, QueryTransactionResponse?>
{
    private readonly ITransactionRepository _repository;
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly IMapper _mapper;

    public GetTransactionQueryHandler(
        ITransactionRepository repository,
        IExchangeRateProvider exchangeRateProvider,
        IMapper mapper)
    {
        _repository = repository;
        _exchangeRateProvider = exchangeRateProvider;
        _mapper = mapper;
    }

    public async Task<QueryTransactionResponse?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByIdAsync(request.TransactionId);
        if (transaction == null)
            return null;

        var exchangeRates = await _exchangeRateProvider.GetExchangeRatesAsync(request.Country, request.Currency);
        var convertedResult = ExchangeRateSelector.Convert(transaction, exchangeRates);

        var response = _mapper.Map<QueryTransactionResponse>(transaction);
        response = response with
        {
            TaxRate = convertedResult.ExchangeRateUsed,
            ConvertedValue = convertedResult.ConvertedAmount
        };

        // TODO: Phase 2B - publish TransactionConvertedEvent via injected IEventPublisher (deferred)

        return response;
    }
}
