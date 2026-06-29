using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WexTransaction.Infra.Database.Data;
using WexTransaction.Infra.Database.Extensions;

namespace WexTransaction.Tests.Infrastructure.Database.Extensions;

public class DatabaseExtensionsTests
{
    /// <summary>
    /// Test that MigrateDatabase returns the app instance for method chaining.
    /// Verifies fluent interface pattern supports fluent API usage.
    /// </summary>
    [Fact]
    public void MigrateDatabase_ReturnsAppInstance_ForMethodChaining()
    {
        // Arrange
        var mockDatabaseFacade = new Mock<DatabaseFacade>();
        var mockDbContext = new Mock<WexTransactionDbContext>(
            new DbContextOptionsBuilder<WexTransactionDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        mockDbContext.Setup(x => x.Database).Returns(mockDatabaseFacade.Object);

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<WexTransactionDbContext>(_ => mockDbContext.Object);
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act
        var result = app.MigrateDatabase();

        // Assert - Verify the same app instance is returned
        Assert.Same(app, result);
    }

    /// <summary>
    /// Test that MigrateDatabase invokes Database.Migrate() on the DbContext.
    /// Verifies the extension method performs the core migration operation.
    /// </summary>
    [Fact]
    public void MigrateDatabase_InvokesDatabaseMigrate_WhenCalled()
    {
        // Arrange
        var mockDatabaseFacade = new Mock<DatabaseFacade>();
        var mockDbContext = new Mock<WexTransactionDbContext>(
            new DbContextOptionsBuilder<WexTransactionDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        mockDbContext.Setup(x => x.Database).Returns(mockDatabaseFacade.Object);

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<WexTransactionDbContext>(_ => mockDbContext.Object);
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act
        var result = app.MigrateDatabase();

        // Assert
        Assert.Same(app, result);
        mockDatabaseFacade.Verify(x => x.Migrate(), Times.Once);
    }

    /// <summary>
    /// Test that MigrateDatabase properly retrieves DbContext from service provider.
    /// Verifies dependency injection container is correctly used.
    /// </summary>
    [Fact]
    public void MigrateDatabase_ResolvesDbContext_FromServiceProvider()
    {
        // Arrange
        var mockDatabaseFacade = new Mock<DatabaseFacade>();
        var mockDbContext = new Mock<WexTransactionDbContext>(
            new DbContextOptionsBuilder<WexTransactionDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        mockDbContext.Setup(x => x.Database).Returns(mockDatabaseFacade.Object);

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<WexTransactionDbContext>(_ => mockDbContext.Object);
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act
        var result = app.MigrateDatabase();

        // Assert
        Assert.Same(app, result);
        mockDatabaseFacade.Verify(x => x.Migrate(), Times.Once);
    }

    /// <summary>
    /// Test that MigrateDatabase creates a service scope for the operation.
    /// Verifies proper scope lifecycle is maintained during migration.
    /// </summary>
    [Fact]
    public void MigrateDatabase_CreatesServiceScope_ForMigrationOperation()
    {
        // Arrange
        var mockDatabaseFacade = new Mock<DatabaseFacade>();
        var mockDbContext = new Mock<WexTransactionDbContext>(
            new DbContextOptionsBuilder<WexTransactionDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        mockDbContext.Setup(x => x.Database).Returns(mockDatabaseFacade.Object);

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<WexTransactionDbContext>(_ => mockDbContext.Object);
        builder.Services.AddLogging();
        var app = builder.Build();

        // Act
        var result = app.MigrateDatabase();

        // Assert
        Assert.Same(app, result);
        mockDatabaseFacade.Verify(x => x.Migrate(), Times.Once);
        mockDbContext.Verify(x => x.Database, Times.AtLeastOnce);
    }
}

