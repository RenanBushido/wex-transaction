namespace WexTransaction.Api.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseApiCors(this WebApplication app)
    {
        app.UseCors(CorsExtensions.CorsPolicy);
        return app;
    }
}
