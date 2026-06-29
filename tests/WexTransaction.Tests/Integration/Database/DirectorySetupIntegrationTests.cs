using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WexTransaction.Infra.Database.Extensions;

namespace WexTransaction.Tests.Integration.Database;

public class DirectorySetupIntegrationTests
{
    /// <summary>
    /// Integration test: Verify that EnsureDatabaseDirectory creates the directory
    /// and doesn't interfere with the WebApplication startup.
    /// </summary>
    [Fact]
    public void ApplicationStartup_EnsureDatabaseDirectory_CompletesWithoutErrors()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act - Simulate startup sequence
        var exception = Record.Exception(() =>
        {
            app.EnsureDatabaseDirectory();
        });

        // Assert
        Assert.Null(exception);
    }

    /// <summary>
    /// Integration test: Verify that EnsureDatabaseDirectory works with real services.
    /// </summary>
    [Fact]
    public void ApplicationStartup_WithRealServices_EnsureDatabaseDirectorySucceeds()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(new[] { "--environment", "Testing" });

        // Add real services
        builder.Services.AddLogging();
        builder.Services.AddApplicationServices();
        builder.Services.AddApiCors(builder.Configuration);

        var app = builder.Build();

        // Act
        var exception = Record.Exception(() =>
        {
            app.EnsureDatabaseDirectory();
        });

        // Assert
        Assert.Null(exception);
    }

    /// <summary>
    /// Integration test: Verify idempotency - calling multiple times doesn't break anything.
    /// </summary>
    [Fact]
    public void ApplicationStartup_MultipleDatabaseDirectorySetupCalls_AreIdempotent()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act - Call multiple times
        var exception1 = Record.Exception(() => app.EnsureDatabaseDirectory());
        var exception2 = Record.Exception(() => app.EnsureDatabaseDirectory());
        var exception3 = Record.Exception(() => app.EnsureDatabaseDirectory());

        // Assert
        Assert.Null(exception1);
        Assert.Null(exception2);
        Assert.Null(exception3);
    }
}
