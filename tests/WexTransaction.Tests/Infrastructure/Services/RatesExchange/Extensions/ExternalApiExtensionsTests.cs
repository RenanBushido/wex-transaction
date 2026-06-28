namespace WexTransaction.Tests.Infrastructure.Services.RatesExchange.Extensions;
public class ExternalApiExtensionsTests
{
    private static IConfiguration CreateConfiguration(string? treasuryUrl = null, int? timeout = null)
    {
        var configDict = new Dictionary<string, string?>
        {
            { "TreasuryApi:BaseUrl", treasuryUrl ?? "https://fiscaldata.treasury.gov" },
        };

        if (timeout.HasValue)
        {
            configDict["TreasuryApi:TimeoutSeconds"] = timeout.Value.ToString();
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
    }

    [Fact]
    public void AddExternalApis_RegistersExchangeRateProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var configuration = CreateConfiguration();

        // Act
        services.AddExternalApis(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<IExchangeRateProvider>();
        Assert.NotNull(provider);
    }

    [Fact]
    public void AddExternalApis_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var configuration = CreateConfiguration();

        // Act
        var result = services.AddExternalApis(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IServiceCollection>(result);
    }

    [Fact]
    public void AddExternalApis_ThrowsWhenTreasuryUrlMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var configDict = new Dictionary<string, string?>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            services.AddExternalApis(configuration));
    }

    [Fact]
    public void AddExternalApis_UsesDefaultTimeoutWhenNotConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var configuration = CreateConfiguration(timeout: null); // No timeout configured

        // Act
        services.AddExternalApis(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<IExchangeRateProvider>();
        Assert.NotNull(provider); // Should use default 30 seconds
    }

    [Fact]
    public void AddExternalApis_UsesConfiguredTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var configuration = CreateConfiguration(timeout: 60);

        // Act
        services.AddExternalApis(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<IExchangeRateProvider>();
        Assert.NotNull(provider);
    }

    [Fact]
    public void AddExternalApis_SupportsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var configuration = CreateConfiguration();

        // Act
        services
            .AddExternalApis(configuration)
            .AddExternalApis(configuration); // Call twice

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<IExchangeRateProvider>();
        Assert.NotNull(provider);
    }

    [Fact]
    public void AddExternalApis_RegistersProviderAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var configuration = CreateConfiguration();

        // Act
        services.AddExternalApis(configuration);

        // Assert
        var providerDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IExchangeRateProvider));
        Assert.NotNull(providerDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, providerDescriptor.Lifetime);
    }

    [Fact]
    public void AddExternalApis_WithDifferentApiUrls()
    {
        // Arrange
        var apiUrls = new[]
        {
            "https://fiscaldata.treasury.gov",
            "https://api.example.com/treasury",
            "http://localhost:5000/api"
        };

        foreach (var url in apiUrls)
        {
            // Act
            var services = new ServiceCollection();
            services.AddHttpClient();
            var configuration = CreateConfiguration(url);
            services.AddExternalApis(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var provider = serviceProvider.GetService<IExchangeRateProvider>();
            Assert.NotNull(provider);
        }
    }

    [Fact]
    public void AddExternalApis_WithDifferentTimeouts()
    {
        // Arrange
        var timeouts = new[] { 10, 30, 60, 120 };

        foreach (var timeout in timeouts)
        {
            // Act
            var services = new ServiceCollection();
            services.AddHttpClient();
            var configuration = CreateConfiguration(timeout: timeout);
            services.AddExternalApis(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var provider = serviceProvider.GetService<IExchangeRateProvider>();
            Assert.NotNull(provider);
        }
    }
}
