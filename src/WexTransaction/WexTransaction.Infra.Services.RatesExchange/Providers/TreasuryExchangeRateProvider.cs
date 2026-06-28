namespace WexTransaction.Infra.Services.RatesExchange.Providers;

public class TreasuryExchangeRateProvider(ITreasuryExchangeRateClient client) : IExchangeRateProvider
{
    #region Variables
    private readonly ITreasuryExchangeRateClient _client = client;
    private readonly ConcurrentDictionary<string, CachedRates> _cache = new();
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(1);

    #endregion

    #region Public Methods
    public async Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(string country, string currency)
    {
        var cacheKey = $"{country}_{currency}";

        if (_cache.TryGetValue(cacheKey, out var cachedRates) && !cachedRates.IsExpired)
        {
            return cachedRates.Rates;
        }

        try
        {
            var filter = $"country:{country} and currency:{currency}";
            var response = await _client.GetExchangeRatesAsync(filter);

            if (response?.Data == null || response.Data.Count == 0)
                throw new CurrencyConversionUnavailableException(
                    $"No exchange rate found for country '{country}' and currency '{currency}'.");

            var exchangeRates = response.Data
                .Where(d => d.Country != null && d.Currency != null && d.EffectiveDate != null)
                .Select(d =>
                {
                    var effectiveDate = DateTimeOffset.Parse(d.EffectiveDate!);
                    return new ExchangeRate(d.Country!, d.Currency!, d.ExchangeRate, effectiveDate);
                })
                .ToList();

            if (exchangeRates.Count == 0)
                throw new CurrencyConversionUnavailableException(
                    $"No valid exchange rate found for country '{country}' and currency '{currency}'.");

            var cached = new CachedRates(exchangeRates, DateTime.UtcNow);
            _cache.AddOrUpdate(cacheKey, cached, (_, _) => cached);

            return exchangeRates;
        }
        catch (CurrencyConversionUnavailableException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new CurrencyConversionUnavailableException(
                $"Failed to fetch exchange rates from Treasury API: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new CurrencyConversionUnavailableException(
                $"An error occurred while fetching exchange rates: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods
    private class CachedRates(List<ExchangeRate> rates, DateTime cachedAt)
    {
        public List<ExchangeRate> Rates { get; } = rates;
        public DateTime CachedAt { get; } = cachedAt;

        public bool IsExpired => DateTime.UtcNow - CachedAt > TimeSpan.FromHours(1);
    }

    #endregion
}
