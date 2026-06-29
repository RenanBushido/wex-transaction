namespace WexTransaction.CrossCutting.AppDependencies;

public static class LoggingExtensions
{
    public static IServiceCollection AddSerilogLogging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application",
                sp.GetRequiredService<IHostEnvironment>().ApplicationName));

        return services;
    }
}
