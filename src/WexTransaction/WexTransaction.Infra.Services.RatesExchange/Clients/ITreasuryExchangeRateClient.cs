namespace WexTransaction.Infra.Services.RatesExchange.Clients;
public interface ITreasuryExchangeRateClient
{
    [Get("/v1/accounting/od/rates_of_exchange")]
    Task<TreasuryRatesResponse> GetExchangeRatesAsync(
        [Query] string? sort = "effective_date",
        [Query] string? fields = null,
        [Query] string? filter = null,
        [Query] int pageSize = 10000);
}

public class TreasuryRatesResponse
{
    public List<TreasuryRateData>? Data { get; set; }
}

public class TreasuryRateData
{
    public string? Country_Currency_Desc { get; set; }
    public decimal Exchange_Rate { get; set; }
    public string? Record_Date { get; set; }
    public string? Effective_Date { get; set; }
}
