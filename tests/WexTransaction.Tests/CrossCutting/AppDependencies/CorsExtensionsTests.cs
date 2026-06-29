namespace WexTransaction.Tests.CrossCutting.AppDependencies;

public class CorsExtensionsTests
{
    private static IConfiguration CreateConfiguration(
        string[]? origins = null,
        string[]? methods = null,
        string[]? headers = null)
    {
        var dict = new Dictionary<string, string?>();

        if (origins is not null)
            for (int i = 0; i < origins.Length; i++)
                dict[$"Cors:AllowedOrigins:{i}"] = origins[i];

        if (methods is not null)
            for (int i = 0; i < methods.Length; i++)
                dict[$"Cors:AllowedMethods:{i}"] = methods[i];

        if (headers is not null)
            for (int i = 0; i < headers.Length; i++)
                dict[$"Cors:AllowedHeaders:{i}"] = headers[i];

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    private static CorsPolicy GetPolicy(IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        var corsOptions = sp.GetRequiredService<IOptions<CorsOptions>>().Value;
        return corsOptions.GetPolicy(CorsExtensions.CorsPolicy)!;
    }

    [Fact]
    public void AddApiCors_WithSpecificOrigins_RegistersNamedPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration(
            origins: ["https://app.example.com", "https://admin.example.com"],
            methods: ["GET", "POST"],
            headers: ["Content-Type", "Authorization"]);

        // Act
        services.AddApiCors(config);
        var policy = GetPolicy(services);

        // Assert
        Assert.NotNull(policy);
        Assert.Contains("https://app.example.com", policy.Origins);
        Assert.Contains("https://admin.example.com", policy.Origins);
        Assert.False(policy.AllowAnyOrigin);
    }

    [Fact]
    public void AddApiCors_WithWildcardOrigin_UsesAllowAnyOrigin()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration(
            origins: ["*"],
            methods: ["*"],
            headers: ["*"]);

        // Act
        services.AddApiCors(config);
        var policy = GetPolicy(services);

        // Assert
        Assert.NotNull(policy);
        Assert.True(policy.AllowAnyOrigin);
        Assert.True(policy.AllowAnyMethod);
        Assert.True(policy.AllowAnyHeader);
    }

    [Fact]
    public void AddApiCors_WithEmptyOrigins_RegistersRestrictivePolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration(); // no origins configured

        // Act
        services.AddApiCors(config);
        var policy = GetPolicy(services);

        // Assert
        Assert.NotNull(policy);
        Assert.Empty(policy.Origins);
        Assert.False(policy.AllowAnyOrigin);
    }

    [Fact]
    public void AddApiCors_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration(origins: ["https://example.com"]);

        // Act
        var result = services.AddApiCors(config);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void CorsPolicy_ConstantMatchesPolicyRegisteredByAddApiCors()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = CreateConfiguration(origins: ["https://example.com"]);

        // Act
        services.AddApiCors(config);
        var sp = services.BuildServiceProvider();
        var corsOptions = sp.GetRequiredService<IOptions<CorsOptions>>().Value;

        // Assert — the constant used by UseApiCors must resolve the policy registered by AddApiCors
        var policy = corsOptions.GetPolicy(CorsExtensions.CorsPolicy);
        Assert.NotNull(policy);
    }
}
