namespace WexTransaction.CrossCutting.AppDependencies;

public static class ExternalApiExtensions
{
    public static IServiceCollection AddExternalApis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var treasuryApiUrl = configuration["TreasuryApi:BaseUrl"]
            ?? throw new InvalidOperationException("Treasury API URL configuration 'TreasuryApi:BaseUrl' not found.");

        var timeoutSeconds = configuration.GetValue<int?>("TreasuryApi:TimeoutSeconds") ?? 30;

        services.AddRefitClient<ITreasuryExchangeRateClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(treasuryApiUrl);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            })
            .AddTreasuryApiPolicies();

        services.AddScoped<IExchangeRateProvider, TreasuryExchangeRateProvider>();

        return services;
    }
}
