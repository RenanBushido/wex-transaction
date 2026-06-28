namespace WexTransaction.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Validating request: {typeof(TRequest).Name}");
        // TODO: Phase 2B - Add FluentValidation integration
        return await next();
    }
}
