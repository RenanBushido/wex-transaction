namespace WexTransaction.Infra.Services.RatesExchange.Extensions;

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

public static class ResiliencePoliciesExtensions
{
    private const int DefaultRetryAttempts = 3;
    private const int DefaultTimeoutSeconds = 10;
    private const int CircuitBreakerThreshold = 5;
    private const int CircuitBreakerTimeoutSeconds = 30;

    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<OperationCanceledException>()
            .OrResult<HttpResponseMessage>(r =>
                r.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                r.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
            .WaitAndRetryAsync(
                retryCount: DefaultRetryAttempts,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timespan, attemptNumber, context) =>
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Retry {attemptNumber} after {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<OperationCanceledException>()
            .OrResult<HttpResponseMessage>(r =>
                r.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                r.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: CircuitBreakerThreshold,
                durationOfBreak: TimeSpan.FromSeconds(CircuitBreakerTimeoutSeconds),
                onBreak: (outcome, timespan) =>
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Circuit breaker opened for {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    System.Diagnostics.Debug.WriteLine("Circuit breaker reset");
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(DefaultTimeoutSeconds),
            TimeoutStrategy.Optimistic);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        return Policy.WrapAsync(
            GetTimeoutPolicy(),
            GetRetryPolicy(),
            GetCircuitBreakerPolicy());
    }

    public static IHttpClientBuilder AddTreasuryApiPolicies(
        this IHttpClientBuilder httpClientBuilder)
    {
        var combinedPolicy = GetCombinedPolicy();

        return httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            new PolicyHttpMessageHandler(combinedPolicy, new HttpClientHandler()));
    }
}

internal class PolicyHttpMessageHandler : DelegatingHandler
{
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;

    public PolicyHttpMessageHandler(IAsyncPolicy<HttpResponseMessage> policy, HttpMessageHandler innerHandler)
    {
        _policy = policy;
        InnerHandler = innerHandler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _policy.ExecuteAsync(
            async ct => await base.SendAsync(request, ct),
            cancellationToken);
    }
}
