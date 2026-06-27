namespace WexTransaction.Application.Services;

using WexTransaction.Domain.ValueObjects;

public interface IExchangeRateProvider
{
    Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(string country, string currency);
}
