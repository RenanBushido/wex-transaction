using Microsoft.Extensions.DependencyInjection;
using WexTransaction.Application.Mappings;

namespace WexTransaction.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));

        services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
        services.AddScoped<IQueryTransactionUseCase, GetTransactionUseCase>();

        return services;
    }
}
