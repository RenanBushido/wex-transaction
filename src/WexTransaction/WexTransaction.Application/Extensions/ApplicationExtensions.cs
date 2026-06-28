namespace WexTransaction.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));

        // Query Services (Port implementations from Application layer)
        services.AddScoped<ITransactionQueryService, TransactionQueryService>();

        // Legacy Use Cases (backward compatibility, to be deprecated in Phase 2C)
        services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
        services.AddScoped<IQueryTransactionUseCase, GetTransactionUseCase>();

        return services;
    }
}
