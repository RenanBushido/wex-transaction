namespace WexTransaction.Tests.Infrastructure.Database.Extensions;

public class PersistenceExtensionsTests
{
    private static IConfiguration CreateConfiguration(string? connectionString = null)
    {
        var configDict = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", connectionString ?? "Server=localhost;Database=test;User Id=test;Password=test;" }
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
    }

    [Fact]
    public void AddPersistence_RegistersDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddPersistence(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetService<WexTransactionDbContext>();
        Assert.NotNull(dbContext);
    }

    [Fact]
    public void AddPersistence_RegistersTransactionRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddPersistence(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var repository = serviceProvider.GetService<ITransactionRepository>();
        Assert.NotNull(repository);
    }

    [Fact]
    public void AddPersistence_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        var result = services.AddPersistence(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IServiceCollection>(result);
    }

    [Fact]
    public void AddPersistence_ThrowsWhenConnectionStringMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var configDict = new Dictionary<string, string?>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            services.AddPersistence(configuration));
    }

    [Fact]
    public void AddPersistence_SupportsMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services
            .AddPersistence(configuration)
            .AddPersistence(configuration); // Call twice

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetService<WexTransactionDbContext>();
        Assert.NotNull(dbContext);
    }

    [Fact]
    public void AddPersistence_RegistersRepositoryAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddPersistence(configuration);

        // Assert
        var repositoryDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(ITransactionRepository));
        Assert.NotNull(repositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, repositoryDescriptor.Lifetime);
    }

    [Fact]
    public void AddPersistence_WithDifferentConnectionStrings()
    {
        // Arrange
        var connectionStrings = new[]
        {
            "Server=localhost;Database=test1;User Id=user1;Password=pass1;",
            "Server=remotehost;Database=prod;User Id=produser;Password=prodpass;",
            "Server=testserver;Port=5433;Database=testdb;User Id=testuser;Password=testpass;"
        };

        foreach (var connectionString in connectionStrings)
        {
            // Act
            var services = new ServiceCollection();
            var configuration = CreateConfiguration(connectionString);
            services.AddPersistence(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<WexTransactionDbContext>();
            Assert.NotNull(dbContext);
        }
    }
}
