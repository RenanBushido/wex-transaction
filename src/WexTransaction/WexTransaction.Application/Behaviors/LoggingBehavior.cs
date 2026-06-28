using System.Diagnostics;

namespace WexTransaction.Application.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior: Logging cross-cutting concern.
///
/// Logs request entry, execution time, and success status.
/// Catches and logs exceptions, then re-throws for error handling behavior.
///
/// Performance: Target < 2ms overhead via efficient Stopwatch timing.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            System.Diagnostics.Debug.WriteLine($"Processing request: {requestType} at {DateTimeOffset.UtcNow:O}");

            var response = await next();

            stopwatch.Stop();
            var responseType = typeof(TResponse).Name;
            System.Diagnostics.Debug.WriteLine(
                $"Request completed successfully: {requestType} → {responseType} in {stopwatch.ElapsedMilliseconds}ms");

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine(
                $"Request failed: {requestType} after {stopwatch.ElapsedMilliseconds}ms. Exception: {ex.Message}");

            throw;
        }
    }
}
