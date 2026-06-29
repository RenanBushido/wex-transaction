namespace WexTransaction.Domain.Interfaces;

public interface IExchangeRateProvider
{
    Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(string date, string country, string currency);
}
