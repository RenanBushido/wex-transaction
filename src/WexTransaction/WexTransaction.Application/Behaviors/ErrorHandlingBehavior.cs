namespace WexTransaction.Application.Behaviors;

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
            
            throw;
        }
    }
}
