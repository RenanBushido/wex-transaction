namespace WexTransaction.Application.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior: Validation cross-cutting concern (extensible placeholder).
///
/// Phase 2B: Basic structure, logs validation attempts.
/// Phase 2C+: Will integrate FluentValidation validators for command validation.
///
/// Queries typically don't require validation (read operations).
/// Commands may require validation (write operations).
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;

        System.Diagnostics.Debug.WriteLine($"Validating request: {requestType}");

        // TODO: Phase 2C - Integrate FluentValidation validators
        // Example:
        // var validationResults = await _validator.ValidateAsync(request, cancellationToken);
        // if (!validationResults.IsValid)
        //     throw new ValidationException(validationResults.Errors);

        return await next();
    }
}
