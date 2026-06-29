using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WexTransaction.CrossCutting.AppDependencies;

namespace WexTransaction.Tests.CrossCutting.AppDependencies;

public class LoggingExtensionsTests
{
    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Serilog:MinimumLevel:Default", "Information" }
            })
            .Build();

    [Fact]
    public void AddSerilogLogging_RegistersSerilogProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(
            new TestHostEnvironment("WexTransaction.Api"));
        var configuration = CreateConfiguration();

        services.AddSerilogLogging(configuration);
        var provider = services.BuildServiceProvider();

        var loggerFactory = provider.GetService<ILoggerFactory>();
        Assert.NotNull(loggerFactory);
    }

    [Fact]
    public void AddSerilogLogging_ReturnsServiceCollectionForChaining()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(
            new TestHostEnvironment("WexTransaction.Api"));
        var configuration = CreateConfiguration();

        var result = services.AddSerilogLogging(configuration);

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IServiceCollection>(result);
    }

    [Fact]
    public void AddSerilogLogging_ReadsFromConfiguration()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(
            new TestHostEnvironment("WexTransaction.Api"));
        var configuration = CreateConfiguration();

        var exception = Record.Exception(() =>
        {
            services.AddSerilogLogging(configuration);
            services.BuildServiceProvider();
        });

        Assert.Null(exception);
    }

    private sealed class TestHostEnvironment(string applicationName)
        : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public string ApplicationName { get; set; } = applicationName;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Test";
    }
}
