namespace WexTransaction.Domain.Interfaces;

public interface IExchangeRateProvider
{
    Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(string country, string currency);
}
