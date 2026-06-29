namespace WexTransaction.Application.UseCases.GetPurchaseTransaction;

public sealed class GetPurchaseTransactionHandler(
    ITransactionDapperRepository repository,
    IExchangeRateProvider exchangeRate
) : IRequestHandler<GetPurchaseTransactionRequest, GetPurchaseTransactionResponse>
{
    #region Variables

    private readonly ITransactionDapperRepository _repository = repository;
    private readonly IExchangeRateProvider _exchangeRate = exchangeRate;

    #endregion

    #region Public Method
    public async Task<GetPurchaseTransactionResponse> Handle(GetPurchaseTransactionRequest request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByIdAsync(request.TransactionId);

        if(transaction is null) return null!;

        var exchangeRates = await _exchangeRate.GetExchangeRatesAsync(request.Country, request.Currency);
        var convertResult = ExchangeRateSelector.Convert(transaction, exchangeRates);

        return new GetPurchaseTransactionResponse(
            TransactionId: transaction.Id,
            Description: transaction.Description,
            Date: convertResult.TransactionDate.Date,
            Amount: transaction.Amount,
            TaxRate: convertResult.ExchangeRateUsed,
            ConvertedValue: convertResult.ConvertedAmount
        );
    }

    #endregion
}