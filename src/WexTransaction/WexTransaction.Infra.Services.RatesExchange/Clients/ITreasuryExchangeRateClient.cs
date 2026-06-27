namespace WexTransaction.Infra.Services.RatesExchange.Clients;
public interface ITreasuryExchangeRateClient
{
    [Get("/v1/accounting/od/rates_of_exchange")]
    Task<TreasuryRatesResponse> GetExchangeRatesAsync(
        [Query] string filter,
        [Query] string sort = "-effective_date",
        [Query] int pageSize = 10000);
}

public class TreasuryRatesResponse
{
    public List<TreasuryRateData>? Data { get; set; }
}

public class TreasuryRateData
{
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public decimal ExchangeRate { get; set; }
    public string? EffectiveDate { get; set; }
}
