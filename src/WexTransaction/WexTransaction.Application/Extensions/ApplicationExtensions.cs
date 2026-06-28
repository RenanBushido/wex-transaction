namespace WexTransaction.Application.Extensions;

using AutoMapper;
using WexTransaction.Application.Mappings;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
        });

        return services;
    }
}
