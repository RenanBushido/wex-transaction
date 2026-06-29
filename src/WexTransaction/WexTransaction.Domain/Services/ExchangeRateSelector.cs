namespace WexTransaction.Domain.Services;

public static class ExchangeRateSelector
{
    public static ConvertedTransactionResult Convert(        
        PurchaseTransaction transaction,
        IEnumerable<ExchangeRate> rates)
    {
        var lowerBound = transaction.TransactionDate.AddMonths(-6);

        var selectedRate = rates
            .Where(r => r.EffectiveDate <= transaction.TransactionDate && r.EffectiveDate >= lowerBound)
            .OrderByDescending(r => r.EffectiveDate)
            .Cast<ExchangeRate?>()
            .FirstOrDefault();

        if (selectedRate is null)
            throw new CurrencyConversionUnavailableException(
                "No exchange rate is available within 6 months of the purchase date.");

        var convertedAmount = Math.Round(
            transaction.Amount.Value * selectedRate.Value.Rate,
            2,
            MidpointRounding.AwayFromZero);

        return new ConvertedTransactionResult(
            transaction.Id,
            transaction.Description,
            transaction.TransactionDate,
            transaction.Amount.Value,
            selectedRate.Value.Rate,
            convertedAmount);
    }
}
