using System.Globalization;
using Microsoft.Extensions.Logging;

namespace WexTransaction.Application.UseCases.GetPurchaseTransaction;

public sealed class GetPurchaseTransactionHandler(
    ITransactionDapperRepository repository,
    IExchangeRateProvider exchangeRate,
    ILogger<GetPurchaseTransactionHandler> logger
) : IRequestHandler<GetPurchaseTransactionRequest, GetPurchaseTransactionResponse>
{
    #region Variables

    private readonly ITransactionDapperRepository _repository = repository;
    private readonly IExchangeRateProvider _exchangeRate = exchangeRate;
    private readonly ILogger<GetPurchaseTransactionHandler> _logger = logger;

    #endregion

    #region Public Method
    public async Task<GetPurchaseTransactionResponse> Handle(GetPurchaseTransactionRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting transaction {TransactionId}", request.TransactionId);

        var transaction = await _repository.GetByIdAsync(request.TransactionId);

        if(transaction is null)
        {
             _logger.LogWarning("Transaction {TransactionId} not found", request.TransactionId);

            return null!;
        }

        var exchangeRates = await _exchangeRate.GetExchangeRatesAsync(transaction.TransactionDate.ToString("yyyy-MM-dd"), request.Country, request.Currency);
        var convertResult = ExchangeRateSelector.Convert(transaction, exchangeRates);

         _logger.LogInformation("Transaction {TransactionId} retrieved successfully", request.TransactionId);

        return new GetPurchaseTransactionResponse(
            TransactionId: transaction.Id,
            Description: transaction.Description,
            Date: convertResult.TransactionDate.Date,
            Amount: transaction.Amount.ToString(),
            TaxRate: convertResult.ExchangeRateUsed.ToString(),
            ConvertedValue: convertResult.ConvertedAmount.ToString()
        );
    }

    #endregion
}