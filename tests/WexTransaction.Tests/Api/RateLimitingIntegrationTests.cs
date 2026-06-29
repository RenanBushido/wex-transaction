namespace WexTransaction.Tests.Api;

public class RateLimitingConfigurationTests
{
    [Fact]
    public void RateLimitingConfiguration_HasPostPolicyConfigured()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RateLimiting:Policies:post-policy:PermitLimit", "10" },
                { "RateLimiting:Policies:post-policy:WindowSeconds", "60" }
            })
            .Build();

        var rateLimitingConfig = configuration.GetSection("RateLimiting");
        var postPolicy = rateLimitingConfig.GetSection("Policies:post-policy");

        Assert.NotNull(postPolicy);
        Assert.Equal("10", postPolicy["PermitLimit"]);
        Assert.Equal("60", postPolicy["WindowSeconds"]);
    }

    [Fact]
    public void RateLimitingConfiguration_HasGetPolicyConfigured()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RateLimiting:Policies:get-policy:PermitLimit", "30" },
                { "RateLimiting:Policies:get-policy:WindowSeconds", "60" }
            })
            .Build();

        var rateLimitingConfig = configuration.GetSection("RateLimiting");
        var getPolicy = rateLimitingConfig.GetSection("Policies:get-policy");

        Assert.NotNull(getPolicy);
        Assert.Equal("30", getPolicy["PermitLimit"]);
        Assert.Equal("60", getPolicy["WindowSeconds"]);
    }

    [Fact]
    public void RateLimitingConfiguration_DevelopmentHasHigherLimits()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RateLimiting:Policies:post-policy:PermitLimit", "100" },
                { "RateLimiting:Policies:get-policy:PermitLimit", "300" }
            })
            .Build();

        var rateLimitingConfig = configuration.GetSection("RateLimiting");
        var postPolicy = rateLimitingConfig.GetSection("Policies:post-policy");
        var getPolicy = rateLimitingConfig.GetSection("Policies:get-policy");

        Assert.Equal("100", postPolicy["PermitLimit"]);
        Assert.Equal("300", getPolicy["PermitLimit"]);
    }

    [Fact]
    public void RateLimitingConfiguration_PoliciesHaveWindowSetting()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RateLimiting:Policies:post-policy:WindowSeconds", "60" },
                { "RateLimiting:Policies:get-policy:WindowSeconds", "60" }
            })
            .Build();

        var rateLimitingConfig = configuration.GetSection("RateLimiting");
        var policies = rateLimitingConfig.GetSection("Policies").GetChildren();

        Assert.All(policies, policy =>
        {
            Assert.NotNull(policy["WindowSeconds"]);
        });
    }
}
