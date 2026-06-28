namespace WexTransaction.Application.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior: Error handling cross-cutting concern (extensible placeholder).
///
/// Phase 2B: Catches exceptions, logs details, and re-throws for API layer handling.
/// Phase 2C+: Will map domain exceptions to application layer exceptions.
/// Phase 3+: May handle domain-specific error scenarios.
///
/// Actual HTTP response mapping deferred to API layer global exception handler.
/// </summary>
public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestType = typeof(TRequest).Name;

            System.Diagnostics.Debug.WriteLine(
                $"Exception in request handler: {requestType}. " +
                $"Exception Type: {ex.GetType().Name}, Message: {ex.Message}, " +
                $"StackTrace: {ex.StackTrace}");

            // TODO: Phase 2C+ - Map domain exceptions to application layer exceptions
            // Example:
            // if (ex is DomainException domainEx)
            //     throw new ApplicationException(domainEx.Message);

            throw;
        }
    }
}
