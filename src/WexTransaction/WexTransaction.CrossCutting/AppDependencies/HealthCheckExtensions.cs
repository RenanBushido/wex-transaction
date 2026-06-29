namespace WexTransaction.CrossCutting.AppDependencies;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WexTransactionConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'ConnectionStrings:WexTransactionConnection' not found.");

        var treasuryBaseUrl = configuration["TreasuryApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Configuration key 'TreasuryApi:BaseUrl' not found.");

        services.AddHealthChecks()
            .AddNpgSql(
                connectionString,
                name: "postgresql",
                tags: new[] { "ready" })
            .AddUrlGroup(
                new Uri(treasuryBaseUrl),
                name: "treasury-api",
                timeout: TimeSpan.FromSeconds(5),
                tags: new[] { "ready" });

        return services;
    }
}
