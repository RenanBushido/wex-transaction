using System.Globalization;

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

        decimal valueTransaction = decimal.Parse(transaction.Amount.ToString(), CultureInfo.InvariantCulture);
        decimal valueTax = decimal.Parse(selectedRate.Value.Rate.ToString(), CultureInfo.InvariantCulture);

        var convertedAmount = Math.Round(
            valueTransaction * valueTax,
            2,
            MidpointRounding.AwayFromZero);

        return new ConvertedTransactionResult(
            transaction.Id,
            transaction.Description,
            transaction.TransactionDate,
            valueTransaction,
            valueTax,
            convertedAmount);
    }
}
