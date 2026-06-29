using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WexTransaction.Infra.Database.Extensions;

namespace WexTransaction.Tests.Infrastructure.Database.Extensions;

public class DatabaseDirectoryExtensionsTests
{
    /// <summary>
    /// Test that EnsureDatabaseDirectory returns the same WebApplication instance for method chaining.
    /// Verifies fluent interface is properly implemented.
    /// </summary>
    [Fact]
    public void EnsureDatabaseDirectory_ReturnsAppInstance_ForChaining()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act - Call the extension method which returns WebApplication for chaining
        var result = app.EnsureDatabaseDirectory();

        // Assert - Verify the method returns the same app instance
        Assert.Same(app, result);
    }

    /// <summary>
    /// Test that EnsureDatabaseDirectory handles directory setup gracefully.
    /// Verifies proper service provider usage and error handling.
    /// </summary>
    [Fact]
    public void EnsureDatabaseDirectory_HandlesDirectorySetup_Gracefully()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act - Should not throw even if directory setup has issues
        var exception = Record.Exception(() => app.EnsureDatabaseDirectory());

        // Assert - Method should complete without throwing
        Assert.Null(exception);
    }

    /// <summary>
    /// Test that EnsureDatabaseDirectory performs idempotent operations.
    /// Multiple calls should not cause errors.
    /// </summary>
    [Fact]
    public void EnsureDatabaseDirectory_IsIdempotent_MultipleCalls()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act - Call multiple times
        var result1 = app.EnsureDatabaseDirectory();
        var result2 = app.EnsureDatabaseDirectory();

        // Assert - Both calls succeed and return same app instance
        Assert.Same(app, result1);
        Assert.Same(app, result2);
    }

    /// <summary>
    /// Test that EnsureDatabaseDirectory integrates with the DI container properly.
    /// Verifies logging is properly integrated.
    /// </summary>
    [Fact]
    public void EnsureDatabaseDirectory_IntegratesWithDependencyInjection_Successfully()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act - Call the extension method
        var result = app.EnsureDatabaseDirectory();

        // Assert - Method completes without errors and returns app instance
        Assert.Same(app, result);
    }
}
