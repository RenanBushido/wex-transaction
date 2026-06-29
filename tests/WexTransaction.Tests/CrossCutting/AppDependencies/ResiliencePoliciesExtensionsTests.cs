using Microsoft.Extensions.DependencyInjection;
using Polly;
using WexTransaction.CrossCutting.AppDependencies;

namespace WexTransaction.Tests.CrossCutting.AppDependencies;

public class ResiliencePoliciesExtensionsTests
{
    [Fact]
    public void GetRetryPolicy_ReturnsNonNullPolicy()
    {
        var policy = ResiliencePoliciesExtensions.GetRetryPolicy();

        Assert.NotNull(policy);
    }

    [Fact]
    public void GetRetryPolicy_ReturnsAsyncPolicy()
    {
        var policy = ResiliencePoliciesExtensions.GetRetryPolicy();

        Assert.IsAssignableFrom<IAsyncPolicy<HttpResponseMessage>>(policy);
    }

    [Fact]
    public void GetCircuitBreakerPolicy_ReturnsNonNullPolicy()
    {
        var policy = ResiliencePoliciesExtensions.GetCircuitBreakerPolicy();

        Assert.NotNull(policy);
    }

    [Fact]
    public void GetCircuitBreakerPolicy_ReturnsAsyncPolicy()
    {
        var policy = ResiliencePoliciesExtensions.GetCircuitBreakerPolicy();

        Assert.IsAssignableFrom<IAsyncPolicy<HttpResponseMessage>>(policy);
    }

    [Fact]
    public void GetTimeoutPolicy_ReturnsNonNullPolicy()
    {
        var policy = ResiliencePoliciesExtensions.GetTimeoutPolicy();

        Assert.NotNull(policy);
    }

    [Fact]
    public void GetTimeoutPolicy_ReturnsAsyncPolicy()
    {
        var policy = ResiliencePoliciesExtensions.GetTimeoutPolicy();

        Assert.IsAssignableFrom<IAsyncPolicy<HttpResponseMessage>>(policy);
    }

    [Fact]
    public void GetCombinedPolicy_ReturnsNonNullPolicy()
    {
        var policy = ResiliencePoliciesExtensions.GetCombinedPolicy();

        Assert.NotNull(policy);
    }

    [Fact]
    public void GetCombinedPolicy_ReturnsAsyncPolicy()
    {
        var policy = ResiliencePoliciesExtensions.GetCombinedPolicy();

        Assert.IsAssignableFrom<IAsyncPolicy<HttpResponseMessage>>(policy);
    }

    [Fact]
    public void AddTreasuryApiPolicies_ReturnsHttpClientBuilderForChaining()
    {
        var services = new ServiceCollection();
        IHttpClientBuilder? result = null;

        services.AddHttpClient("test", c => c.BaseAddress = new Uri("https://example.com"))
            .ConfigureHttpClient(_ => { });

        var builder = services.AddHttpClient("treasury");
        result = builder.AddTreasuryApiPolicies();

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IHttpClientBuilder>(result);
    }

    [Fact]
    public void AddTreasuryApiPolicies_CanBeCalledOnHttpClientBuilder()
    {
        var services = new ServiceCollection();
        var builder = services.AddHttpClient("treasury-test");

        var exception = Record.Exception(() => builder.AddTreasuryApiPolicies());

        Assert.Null(exception);
    }

    [Fact]
    public async Task GetRetryPolicy_ExecutesSuccessfulRequest()
    {
        var policy = ResiliencePoliciesExtensions.GetRetryPolicy();
        var callCount = 0;

        var result = await policy.ExecuteAsync(async () =>
        {
            callCount++;
            await Task.CompletedTask;
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        });

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetCombinedPolicy_ExecutesSuccessfulRequest()
    {
        var policy = ResiliencePoliciesExtensions.GetCombinedPolicy();

        var result = await policy.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        });

        Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
    }
}
