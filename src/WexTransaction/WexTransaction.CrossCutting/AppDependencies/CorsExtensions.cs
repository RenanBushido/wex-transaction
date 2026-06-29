namespace WexTransaction.CrossCutting.AppDependencies;

public static class CorsExtensions
{
    public const string CorsPolicy = "WexTransactionCorsPolicy";

    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        var allowedMethods = configuration.GetSection("Cors:AllowedMethods").Get<string[]>() ?? [];
        var allowedHeaders = configuration.GetSection("Cors:AllowedHeaders").Get<string[]>() ?? [];

        services.AddCors(options =>
            options.AddPolicy(CorsPolicy, policy =>
            {
                if (allowedOrigins.Contains("*"))
                    policy.AllowAnyOrigin();
                else if (allowedOrigins.Length > 0)
                    policy.WithOrigins(allowedOrigins);

                if (allowedMethods.Contains("*"))
                    policy.AllowAnyMethod();
                else if (allowedMethods.Length > 0)
                    policy.WithMethods(allowedMethods);

                if (allowedHeaders.Contains("*"))
                    policy.AllowAnyHeader();
                else if (allowedHeaders.Length > 0)
                    policy.WithHeaders(allowedHeaders);
            }));

        return services;
    }
}
