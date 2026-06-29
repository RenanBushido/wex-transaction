using Microsoft.Extensions.Options;

namespace WexTransaction.Tests.CrossCutting.AppDependencies;

public class HealthCheckExtensionsTests
{
    private static IConfiguration CreateValidConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:WexTransactionConnection", "Host=localhost;Port=5432;Database=TestDB;User Id=postgres;Password=postgres" },
                { "TreasuryApi:BaseUrl", "https://api.fiscaldata.treasury.gov/services/api/fiscal_service" }
            })
            .Build();

    [Fact]
    public void AddApplicationHealthChecks_ReturnsServiceCollectionForChaining()
    {
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();

        var result = services.AddApplicationHealthChecks(configuration);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IServiceCollection>(result);
    }

    [Fact]
    public void AddApplicationHealthChecks_ThrowsWhenConnectionStringMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "TreasuryApi:BaseUrl", "https://api.fiscaldata.treasury.gov/services/api/fiscal_service" }
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() =>
            services.AddApplicationHealthChecks(configuration));
    }

    [Fact]
    public void AddApplicationHealthChecks_ThrowsWhenTreasuryBaseUrlMissing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:WexTransactionConnection", "Host=localhost;Port=5432;Database=TestDB;User Id=postgres;Password=postgres" }
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() =>
            services.AddApplicationHealthChecks(configuration));
    }

    [Fact]
    public void AddApplicationHealthChecks_WithValidConfiguration_RegistersHealthCheckService()
    {
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();

        services.AddApplicationHealthChecks(configuration);

        // HealthCheckService is registered when AddHealthChecks() is called
        Assert.Contains(services, sd =>
            sd.ServiceType.FullName != null &&
            sd.ServiceType.FullName.Contains("HealthCheckService"));
    }

    [Fact]
    public void AddApplicationHealthChecks_WithValidConfiguration_RegistersMultipleChecks()
    {
        var services = new ServiceCollection();
        var configuration = CreateValidConfiguration();

        services.AddApplicationHealthChecks(configuration);

        // Each .AddNpgSql() / .AddUrlGroup() adds an IConfigureOptions<HealthCheckServiceOptions>
        var checkRegistrations = services
            .Where(sd => sd.ServiceType.FullName != null
                && sd.ServiceType.FullName.Contains("IConfigureOptions")
                && (sd.ImplementationInstance?.GetType().FullName?.Contains("HealthCheck") == true
                    || sd.ImplementationFactory != null))
            .ToList();

        Assert.True(services.Count(sd =>
            sd.ServiceType.Name == "IConfigureOptions`1" &&
            sd.ServiceType.FullName!.Contains("HealthCheckServiceOptions")) >= 2,
            "Expected at least 2 health check configurations registered.");
    }
}
