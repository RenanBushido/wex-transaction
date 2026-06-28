namespace WexTransaction.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Debug.WriteLine($"Processing request: {requestType} at {DateTimeOffset.UtcNow:O}");

            var response = await next();

            stopwatch.Stop();
            var responseType = typeof(TResponse).Name;
            Debug.WriteLine(
                $"Request completed successfully: {requestType} → {responseType} in {stopwatch.ElapsedMilliseconds}ms");

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Debug.WriteLine(
                $"Request failed: {requestType} after {stopwatch.ElapsedMilliseconds}ms. Exception: {ex.Message}");

            throw;
        }
    }
}
