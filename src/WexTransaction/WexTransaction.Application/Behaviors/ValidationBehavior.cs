namespace WexTransaction.Application.Behaviors;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    #region Variables

    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    #endregion

    #region Public Methods
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if(_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults.SelectMany(r => r.Errors)
                                                    .Where(f => f != null).ToList();

            if(failures.Count != 0)
                throw new FluentValidation.ValidationException(failures);
        }

        return await next();
    }

    #endregion
}