namespace WexTransaction.Api.Exceptions;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    #region Variables
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;

    #endregion

    #region Public Methods
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException)
            return false;

        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,            
            InvalidAmountException => StatusCodes.Status400BadRequest,
            InvalidDescriptionException => StatusCodes.Status400BadRequest,
            InvalidTransactionDateException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status422UnprocessableEntity
        };

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode == StatusCodes.Status400BadRequest ? "Bad Request" : "Unprocessable Entity",
                Detail = exception.Message
            }
        });
    }

    #endregion
}
