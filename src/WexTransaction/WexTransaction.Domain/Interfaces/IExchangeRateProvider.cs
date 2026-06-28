namespace WexTransaction.Domain.Interfaces;

using WexTransaction.Domain.ValueObjects;

/// <summary>
/// Port: Provedor de taxas de câmbio.
/// Implementação: TreasuryExchangeRateProvider (Infrastructure layer).
/// </summary>
public interface IExchangeRateProvider
{
    Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(string country, string currency);
}
