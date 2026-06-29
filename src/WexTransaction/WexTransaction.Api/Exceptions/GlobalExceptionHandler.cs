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
        int statusCode;

        if (exception is DomainException domainException)
        {
            statusCode = domainException switch
            {
                InvalidAmountException => StatusCodes.Status400BadRequest,
                InvalidDescriptionException => StatusCodes.Status400BadRequest,
                InvalidTransactionDateException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status417ExpectationFailed
            };
        }
        else if (exception is FluentValidation.ValidationException)
        {
            statusCode = StatusCodes.Status400BadRequest;
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode),
                Detail = exception.Message
            }
        });
    }

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status417ExpectationFailed => "Expectation failed",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Error"
        };

    #endregion
}
