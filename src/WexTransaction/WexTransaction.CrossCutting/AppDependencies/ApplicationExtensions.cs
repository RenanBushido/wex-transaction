namespace WexTransaction.CrossCutting.AppDependencies;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var wexHandlers =AppDomain.CurrentDomain.Load("WexTransaction.Application");

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(wexHandlers);
            config.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        return services;
    }
}