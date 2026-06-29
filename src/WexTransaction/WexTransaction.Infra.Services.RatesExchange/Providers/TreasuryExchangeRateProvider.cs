using Microsoft.Extensions.Logging;

namespace WexTransaction.Infra.Services.RatesExchange.Providers;

public class TreasuryExchangeRateProvider(
    ITreasuryExchangeRateClient client,
    ILogger<TreasuryExchangeRateProvider> logger) : IExchangeRateProvider
{
    #region Variables
    private readonly ITreasuryExchangeRateClient _client = client;
    private readonly ConcurrentDictionary<string, CachedRates> _cache = new();
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(1);
    private readonly ILogger<TreasuryExchangeRateProvider> _logger = logger;

    #endregion

    #region Public Methods
    public async Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(string date, string country, string currency)
    {
        var cacheKey = $"{country}_{currency}";

        if (_cache.TryGetValue(cacheKey, out var cachedRates) && !cachedRates.IsExpired)
        {
            return cachedRates.Rates;
        }

        try
        {
            _logger.LogInformation("Fetching exchange rates for {Country}-{Currency} on {Date}", country, currency, date);

            var fields = $"&fields=country_currency_desc,exchange_rate,record_date,effective_date";
            var filter = $"&filter=country_currency_desc:in:({Capitalize(country)}-{Capitalize(currency)}),record_date:lte:{date}";
            var sort = $"?sort=-record_date";

            var response = await _client.GetExchangeRatesAsync("", fields, filter, 1);

            if (response?.Data == null || response.Data.Count == 0)
            {
                _logger.LogWarning("No exchange rate found for {Country}-{Currency}", country, currency);

                throw new CurrencyConversionUnavailableException(
                    $"No exchange rate found for country '{country}' and currency '{currency}'.");
            }

            var exchangeRates = response.Data
                .Where(d => d.Country_Currency_Desc != null && d.Exchange_Rate != 0 && d.Effective_Date != null)
                .Select(d =>
                {
                    var effectiveDate = DateTimeOffset.Parse(d.Effective_Date!);
                    var splitCountry = d.Country_Currency_Desc!.Split('-');
                    return new ExchangeRate(splitCountry[0] ?? string.Empty,
                                splitCountry[1] ?? string.Empty,
                                d.Exchange_Rate,
                                effectiveDate);
                })
                .ToList();

            if (exchangeRates.Count == 0)
            {
                _logger.LogWarning("No valid exchange rate found for {Country}-{Currency}", country, currency);

                throw new CurrencyConversionUnavailableException(
                    $"No valid exchange rate found for country '{country}' and currency '{currency}'.");
            }

            var cached = new CachedRates(exchangeRates, DateTime.UtcNow);
            _cache.AddOrUpdate(cacheKey, cached, (_, _) => cached);

            _logger.LogInformation("Exchange rates loaded for {Country}-{Currency}: {Count} record(s)", country, currency, exchangeRates.Count);

            return exchangeRates;
        }
        catch (CurrencyConversionUnavailableException cce)
        {
            _logger.LogError(cce, "Currency conversion unavailable for {Country}-{Currency}", country, currency);

            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rates from Treasury API for {Country}-{Currency}", country, currency);

            throw new CurrencyConversionUnavailableException(
                $"Failed to fetch exchange rates from Treasury API: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching exchange rates for {Country}-{Currency}", country, currency);

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

    private static string Capitalize(string str)
    {
        if (str == null) return null!;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str[1..];

        return str.ToUpper();
    }

    #endregion
}
